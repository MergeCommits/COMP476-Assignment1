using UnityEngine;

public class Steering : BehaveType {
    private const float WANDER_OFFSET = 1f;
    private const float WANDER_RADIUS = 5f;
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
            ? input.maxSpeed
            : input.maxSpeed * distance / SLOW_RADIUS;

        Vector2 targetVelocity = direction;
        targetVelocity = targetVelocity.normalized;
        targetVelocity *= targetSpeed;

        Vector2 acceleration = targetVelocity - input.velocity;
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

    public float PerformAlign(SteeringInput input) {
        float rotation = input.targetOrientation - input.orientation;
        rotation = MapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        if (rotationSize < SATISFACTION_RADIUS) {
            return 0f;
        }

        float targetRotation = 0f;
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
            angularAcceleration /= angularAcceleration;
            angularAcceleration *= input.maxAngularAcceleration;
        }

        return angularAcceleration;
    }

    public float PerformFace(SteeringInput input) {
        Vector2 direction = input.targetPosition - input.position;
        if (Mathf.Approximately(direction.sqrMagnitude, 0f)) {
            return 0f;
        }

        input.targetOrientation = Mathf.Atan2(-direction.x, direction.y);
        return PerformAlign(input);
    }

    public float PerformFaceAway(SteeringInput input) => -PerformFace(input);

    public float PerformLookWhereYouGoing(SteeringInput input) {
        Vector2 direction = input.targetPosition - input.position;
        if (Mathf.Approximately(input.velocity.sqrMagnitude, 0f)) {
            return 0f;
        }

        input.targetOrientation = Mathf.Atan2(-input.velocity.x, input.velocity.y);
        return PerformAlign(input);
    }

    #endregion Angular Acceleration
}
