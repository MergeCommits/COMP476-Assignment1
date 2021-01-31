using System.Net;
using UnityEngine;

public class Kinematic : BehaveType {
    
    private float GetNewOrientation(float currentOrientation, Vector2 velocity) =>
        velocity.sqrMagnitude > 0f
            ? Mathf.Atan2(velocity.y, velocity.x)
            : currentOrientation;

    public KinematicOutput PerformSeek(KinematicInput input) {
        Vector2 velocity = input.targetPosition - input.position;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformFlee(KinematicInput input) {
        Vector2 velocity = input.position - input.targetPosition;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformArrive(KinematicInput input) {
        Vector2 velocity = input.targetPosition - input.position;
        if (velocity.sqrMagnitude < SATISFACTION_RADIUS * SATISFACTION_RADIUS) {
            return new KinematicOutput{orientation = input.orientation};
        }

        velocity /= TIME_TO_TARGET;
        float orientation = GetNewOrientation(input.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformPursue(KinematicInput input) {
        Vector2 direction = input.targetPosition - input.position;
        float distance = direction.magnitude;
        float speed = input.velocity.magnitude;

        float predictionTime = speed <= distance / MAX_PREDICTION
            ? MAX_PREDICTION
            : distance / speed;

        input.targetPosition = input.targetVelocity * predictionTime;
        return PerformSeek(input);
    }

    public KinematicOutput PerformWander(KinematicInput input) {
        Vector2 velocity = new Vector2(input.maxSpeed, input.maxSpeed) * OrientationAsVector(input.orientation);
        float orientation = input.orientation;
        float rotation = RandomBinomial();

        return new KinematicOutput{velocity = velocity, orientation = orientation, rotation = rotation};
    }
}
