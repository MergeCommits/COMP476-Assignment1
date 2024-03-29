using System;
using System.Net;
using UnityEngine;

public class Kinematic : BehaveType {
    private float GetNewOrientation(float currentOrientation, Vector2 velocity) =>
        velocity.sqrMagnitude > 0f
            ? Mathf.Atan2(velocity.y, velocity.x)
            : currentOrientation;

    private KinematicOutput PerformSeek(KinematicInput input) {
        Vector2 velocity = input.targetPosition - input.position;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput {velocity = velocity, orientation = orientation};
    }

    private KinematicOutput PerformFlee(KinematicInput input) {
        Vector2 velocity = input.position - input.targetPosition;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput {velocity = velocity, orientation = orientation};
    }

    private KinematicOutput PerformArrive(KinematicInput input) {
        Vector2 velocity = input.targetPosition - input.position;
        if (velocity.sqrMagnitude < SATISFACTION_RADIUS * SATISFACTION_RADIUS) {
            return new KinematicOutput {orientation = input.orientation};
        }

        velocity /= TIME_TO_TARGET;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput {velocity = velocity, orientation = orientation};
    }

    private KinematicOutput PerformPursue(KinematicInput input) {
        Vector2 direction = input.targetPosition - input.position;
        float distance = direction.magnitude;
        float speed = input.velocity.magnitude;

        float predictionTime = speed <= distance / MAX_PREDICTION
            ? MAX_PREDICTION
            : distance / speed;

        input.targetPosition += input.targetVelocity * predictionTime;
        return PerformSeek(input);
    }

    private KinematicOutput PerformWander(KinematicInput input) {
        Vector2 velocity = new Vector2(input.maxVelocity, input.maxVelocity) * OrientationAsVector(input.orientation);
        float orientation = input.orientation;
        float rotation = RandomBinomial() * input.maxRotation;

        return new KinematicOutput {velocity = velocity, orientation = orientation, rotation = rotation};
    }

    private KinematicInput BuildInput(Follower follower) {
        Vector2 targetPosition = follower.hasFollowerScript
            ? follower.followerTarget.position
            : Vector2.zero;
        if (!follower.hasFollowerScript && follower.target != null) {
            targetPosition = follower.target.transform.position.XZ();
        }
        
        Vector2 targetVelocity = follower.hasFollowerScript
            ? follower.followerTarget.velocity
            : Vector2.zero;
        
            
        KinematicInput followerInput = new KinematicInput {
            position = follower.position,
            velocity = follower.velocity,
            orientation = follower.orientation,
            maxVelocity = follower.maxVelocity,
            maxRotation = follower.maxRotation,
            targetPosition = targetPosition,
            targetVelocity = targetVelocity
        };

        return followerInput;
    }

    private void ApplyOutput(Follower follower, KinematicOutput kinematicOutput) {
        follower.position += follower.velocity * Time.deltaTime;
        follower.orientation = kinematicOutput.orientation;
        follower.orientation += follower.rotation * Time.deltaTime;

        Transform transform1 = follower.transform;
        transform1.position = new Vector3(follower.position.x, transform1.position.y, follower.position.y);
        transform1.eulerAngles = new Vector3(0f, -follower.orientation * Mathf.Rad2Deg, 0f);

        follower.velocity = kinematicOutput.velocity;
        if (follower.velocity.magnitude > follower.maxVelocity) {
            follower.velocity = follower.velocity.normalized;
            follower.velocity *= follower.maxVelocity;
        }

        follower.rotation = Mathf.Clamp(kinematicOutput.rotation, -follower.maxRotation, follower.maxRotation);
    }

    public override void UpdateTargetHunt(Follower follower, bool fleeing) {
        KinematicInput followerInput = BuildInput(follower);

        KinematicOutput output = new KinematicOutput();
        if (fleeing) {
            // C.1
            if (Vector2.Distance(followerInput.position, followerInput.targetPosition) < SLOW_RADIUS) {
                // Debug.Log("C1");
                output = PerformFlee(followerInput);
                output.orientation = followerInput.orientation;
                output.rotation = 0f;
            } else {
                // C.2
                // Debug.Log("C2");

                Vector2 targetRotationVect = followerInput.targetPosition - followerInput.position;
                float targetOrientation = Mathf.Atan2(targetRotationVect.y, targetRotationVect.x) + Mathf.PI;

                const float ANGLE_TOLERANCE = 10f;
                if (AngleDifferenceNegligible(followerInput.orientation, targetOrientation, ANGLE_TOLERANCE)) {
                    output = PerformFlee(followerInput);
                } else {
                    output.velocity = Vector2.zero;
                    output.orientation = Mathf.LerpAngle(followerInput.orientation * Mathf.Rad2Deg,
                        targetOrientation * Mathf.Rad2Deg, Time.deltaTime * 5f);
                    output.orientation *= Mathf.Deg2Rad;
                    output.rotation = 0f;
                }
            }
        } else if (followerInput.velocity.magnitude < SLOW_SPEED) {
            // A.1
            if (Vector2.Distance(followerInput.position, followerInput.targetPosition) < SLOW_RADIUS) {
                // Debug.Log("A1");
                output = PerformArrive(followerInput);
                output.orientation = followerInput.orientation;
                output.rotation = 0f;
            } else {
                // A.2
                // Debug.Log("A2");

                Vector2 targetRotationVect = followerInput.targetPosition - followerInput.position;
                float targetOrientation = Mathf.Atan2(targetRotationVect.y, targetRotationVect.x);

                const float ANGLE_TOLERANCE = 10f;
                if (AngleDifferenceNegligible(followerInput.orientation, targetOrientation, ANGLE_TOLERANCE)) {
                    output = PerformArrive(followerInput);
                } else {
                    output.velocity = Vector2.zero;
                    output.orientation = Mathf.LerpAngle(followerInput.orientation * Mathf.Rad2Deg,
                        targetOrientation * Mathf.Rad2Deg, Time.deltaTime * 5f);
                    output.orientation *= Mathf.Deg2Rad;
                    output.rotation = 0f;
                }
            }
        } else {
            Vector2 targetRotationVect = followerInput.targetPosition - followerInput.position;
            float targetOrientation = Mathf.Atan2(targetRotationVect.y, targetRotationVect.x);

            const float ANGLE_TOLERANCE = 10f;
            // B.1
            if (AngleDifferenceNegligible(followerInput.orientation, targetOrientation, ANGLE_TOLERANCE)) {
                // Debug.Log("B1");
                output = PerformArrive(followerInput);
            } else {
                // B.2
                // Debug.Log("B2");
                output.velocity = Vector2.zero;
                output.orientation = Mathf.LerpAngle(followerInput.orientation * Mathf.Rad2Deg, targetOrientation,
                    Time.deltaTime * 5f);
                output.orientation *= Mathf.Deg2Rad;
                output.rotation = 0f;
            }
        }

        ApplyOutput(follower, output);
    }

    public override void UpdateTargetPursue(Follower follower) {
        KinematicInput followerInput = BuildInput(follower);
        KinematicOutput output = PerformPursue(followerInput);

        ApplyOutput(follower, output);
    }

    public override void UpdateWander(Follower follower) {
        KinematicInput followerInput = BuildInput(follower);
        KinematicOutput output = PerformWander(followerInput);

        ApplyOutput(follower, output);
    }
}
