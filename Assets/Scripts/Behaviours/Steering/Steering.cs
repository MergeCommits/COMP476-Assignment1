using UnityEngine;

public class Steering : BehaveType {
    private const float WANDER_OFFSET = 1f;
    private const float WANDER_RADIUS = 15f;
    private float wanderOrientation = 0f;

    #region Acceleration

    public SteeringOutput PerformSeek(SteeringInput input) {
        Vector2 acceleration = input.targetPosition - input.position;
        return new SteeringOutput {acceleration = acceleration, angularAcceleration = 0f};
    }

    public SteeringOutput PerformFlee(SteeringInput input) {
        SteeringOutput steering = PerformSeek(input);
        steering.acceleration *= -1;
        return steering;
    }

    public SteeringOutput PerformArrive(SteeringInput input) {
        Vector2 direction = input.targetPosition - input.position;
        float distance = direction.magnitude;

        if (distance < SATISFACTION_RADIUS) {
            return new SteeringOutput();
        }

        float targetSpeed = distance > SLOW_RADIUS
            ? input.maxVelocity
            : input.maxVelocity * distance / SLOW_RADIUS;

        Vector2 targetVelocity = direction;
        targetVelocity = targetVelocity.normalized;
        targetVelocity *= targetSpeed;

        Vector2 acceleration = (targetVelocity - input.velocity) / TIME_TO_TARGET;
        return new SteeringOutput {acceleration = acceleration, angularAcceleration = 0f};
    }

    public SteeringOutput PerformPursue(SteeringInput input) {
        Vector2 direction = input.targetPosition - input.position;
        float distance = direction.magnitude;
        float speed = input.velocity.magnitude;

        float predictionTime = speed <= distance / MAX_PREDICTION
            ? MAX_PREDICTION
            : distance / speed;

        input.targetPosition += input.targetVelocity * predictionTime;
        return PerformSeek(input);
    }

    public SteeringOutput PerformEvade(SteeringInput input) {
        SteeringOutput ouptut = PerformPursue(input);
        ouptut.acceleration *= -1;
        return ouptut;
    }

    public SteeringOutput PerformWander(SteeringInput input) {
        wanderOrientation += RandomBinomial() * input.maxRotation;
        float targetOrientation = wanderOrientation + input.orientation;
        Vector2 target = input.position + (WANDER_OFFSET * OrientationAsVector(input.orientation));
        target += WANDER_RADIUS * OrientationAsVector(targetOrientation);

        input.targetPosition = target;
        input.targetOrientation = targetOrientation;
        float angularAcceleration = PerformFace(input);

        Vector2 acceleration = input.maxAcceleration * OrientationAsVector(input.orientation);
        return new SteeringOutput {acceleration = acceleration, angularAcceleration = angularAcceleration};
    }

    #endregion Acceleration

    #region Angular Acceleration

    private float MapToRange(float rotation) {
        if (rotation > Mathf.PI) {
            return rotation - (2 * Mathf.PI);
        }

        if (rotation < -Mathf.PI) {
            return rotation + (2 * Mathf.PI);
        }

        return rotation;
    }

    private const float SATISFACTION_ORIENTATION = 5f * Mathf.Deg2Rad;
    public float PerformAlign(SteeringInput input) {
        float rotation = input.targetOrientation - input.orientation;
        rotation = MapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        if (rotationSize < SATISFACTION_ORIENTATION) {
            return 0f;
        }

        float targetRotation;
        if (rotationSize > SLOW_RADIUS) {
            targetRotation = input.maxRotation;
        } else {
            targetRotation = input.maxRotation * rotationSize / SLOW_RADIUS;
        }

        targetRotation *= rotation / rotationSize;

        float angularAcceleration = targetRotation - input.rotation;
        angularAcceleration /= TIME_TO_TARGET;

        float absAngular = Mathf.Abs(angularAcceleration);
        if (absAngular > input.maxAngularAcceleration) {
            angularAcceleration /= absAngular;
            angularAcceleration *= input.maxAngularAcceleration;
        }

        return angularAcceleration;
    }

    public float PerformFace(SteeringInput input) {
        Vector2 direction = input.targetPosition - input.position;
        if (Mathf.Approximately(direction.magnitude, 0f)) {
            return 0f;
        }
        
        input.targetOrientation = Mathf.Atan2(direction.y, direction.x);
        return PerformAlign(input);
    }

    public float PerformFaceAway(SteeringInput input) => -PerformFace(input);

    public float PerformLookWhereYouGoing(SteeringInput input) {
        if (Mathf.Approximately(input.velocity.sqrMagnitude, 0f)) {
            return 0f;
        }

        input.targetOrientation = Mathf.Atan2(input.velocity.y, input.velocity.x);
        return PerformAlign(input);
    }

    #endregion Angular Acceleration
    
    private SteeringInput BuildInput(Follower follower) {
        Vector2 targetPosition = follower.hasFollowerScript
            ? follower.followerTarget.position
            : Vector2.zero;
        if (!follower.hasFollowerScript && follower.target != null) {
            targetPosition = follower.target.transform.position.XZ();
        }
        
        Vector2 targetVelocity = follower.hasFollowerScript
            ? follower.followerTarget.velocity
            : Vector2.zero;
        
        float targetOrientation = follower.hasFollowerScript
            ? follower.followerTarget.orientation
            : 0f;
        
            
        SteeringInput followerInput = new SteeringInput {
            position = follower.position,
            velocity = follower.velocity,
            orientation = follower.orientation,
            rotation = follower.rotation,
            maxVelocity = follower.maxVelocity,
            maxRotation = follower.maxRotation,
            maxAcceleration = follower.maxAcceleration,
            maxAngularAcceleration = follower.maxAngularAcceleration,
            targetPosition = targetPosition,
            targetVelocity = targetVelocity,
            targetOrientation = targetOrientation
        };

        return followerInput;
    }

    private void ApplyOutput(Follower follower, SteeringOutput steeringOutput) {
        follower.position += (follower.velocity * Time.deltaTime) + (follower.acceleration * (0.5f * Time.deltaTime * Time.deltaTime));
        follower.orientation += (follower.rotation * Time.deltaTime) +
                                (follower.angularAcceleration * (0.5f * Time.deltaTime * Time.deltaTime));

        Transform transform1 = follower.transform;
        transform1.position = new Vector3(follower.position.x, transform1.position.y, follower.position.y);
        transform1.eulerAngles = new Vector3(0f, -follower.orientation * Mathf.Rad2Deg, 0f);

        follower.velocity += follower.acceleration * Time.deltaTime;
        if (follower.velocity.magnitude > follower.maxVelocity) {
            follower.velocity = follower.velocity.normalized;
            follower.velocity *= follower.maxVelocity;
        }
        
        follower.acceleration = steeringOutput.acceleration;
        if (follower.acceleration.magnitude > follower.maxAcceleration) {
            follower.acceleration = follower.acceleration.normalized;
            follower.acceleration *= follower.maxAcceleration;
        }
        
        follower.rotation += follower.angularAcceleration * Time.deltaTime;
        follower.rotation = Mathf.Clamp(follower.rotation, -follower.maxRotation, follower.maxRotation);

        follower.angularAcceleration = Mathf.Clamp(steeringOutput.angularAcceleration, -follower.maxAngularAcceleration, follower.maxAngularAcceleration);
    }

    public override void UpdateTargetHunt(Follower follower) {
        SteeringInput followerInput = BuildInput(follower);

        SteeringOutput output = new SteeringOutput();
        if (followerInput.velocity.magnitude < SLOW_SPEED) {
            // A.1
            if (Vector2.Distance(followerInput.position, followerInput.targetPosition) < SLOW_RADIUS) {
                // Debug.Log("A1");
                output = PerformArrive(followerInput);
            } else {
                // A.2
                // Debug.Log("A2");

                Vector2 targetRotationVect = followerInput.targetPosition - followerInput.position;
                float targetOrientation = Mathf.Atan2(targetRotationVect.y, targetRotationVect.x);

                const float ANGLE_TOLERANCE = 10f;
                // if (AngleDifferenceNegligible(followerInput.orientation, targetOrientation, ANGLE_TOLERANCE)) {
                    output = PerformArrive(followerInput);
                // } else {
                //     output.velocity = Vector2.zero;
                //     output.orientation = Mathf.LerpAngle(followerInput.orientation * Mathf.Rad2Deg,
                //         targetOrientation * Mathf.Rad2Deg, Time.deltaTime * 5f);
                //     output.orientation *= Mathf.Deg2Rad;
                //     output.rotation = 0f;
                // }
                
                output.angularAcceleration = PerformFace(followerInput);
            }
        } else {
            Vector2 targetRotationVect = followerInput.targetPosition - followerInput.position;
            float targetOrientation = Mathf.Atan2(targetRotationVect.y, targetRotationVect.x);

            output = PerformArrive(followerInput);
            const float ANGLE_TOLERANCE = 10f;
            // B.1
            if (AngleDifferenceNegligible(followerInput.orientation, targetOrientation, ANGLE_TOLERANCE)) {
                // Debug.Log("B1");
                // output = PerformArrive(followerInput);
                output.angularAcceleration = PerformLookWhereYouGoing(followerInput);
            } else {
                // B.2
                // Debug.Log("B2");
                // output.velocity = Vector2.zero;
                // output.orientation = Mathf.LerpAngle(followerInput.orientation * Mathf.Rad2Deg, targetOrientation,
                //     Time.deltaTime * 5f);
                // output.orientation *= Mathf.Deg2Rad;
                // output.rotation = 0f;
                output.angularAcceleration = PerformFace(followerInput);
            }
        }

        ApplyOutput(follower, output);
    }

    public override void UpdateTargetPursue(Follower follower) {
        SteeringInput followerInput = BuildInput(follower);
        SteeringOutput output = PerformPursue(followerInput);
        output.angularAcceleration = PerformLookWhereYouGoing(followerInput);

        ApplyOutput(follower, output);
    }

    public override void UpdateWander(Follower follower) {
        SteeringInput followerInput = BuildInput(follower);
        SteeringOutput output = PerformWander(followerInput);

        ApplyOutput(follower, output);
    }
}
