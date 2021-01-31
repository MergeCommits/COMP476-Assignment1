using UnityEngine;

public class Kinematic : BehaveType {
    private const float TIME_TO_TARGET = 1f;
    private const float SATISFACTION_RADIUS = 0.5f;
    
    private float GetNewOrientation(float currentOrientation, Vector2 velocity) =>
        velocity.sqrMagnitude > 0f
            ? Mathf.Atan2(velocity.y, velocity.x)
            : currentOrientation;

    public KinematicOutput PerformSeek(FollowerInput followerInput) {
        Vector2 velocity = followerInput.targetPosition - followerInput.position;
        float orientation = GetNewOrientation(followerInput.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformFlee(FollowerInput followerInput) {
        Vector2 velocity = followerInput.position - followerInput.targetPosition;
        float orientation = GetNewOrientation(followerInput.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformArrive(FollowerInput followerInput) {
        Vector2 velocity = followerInput.targetPosition - followerInput.position;
        if (velocity.sqrMagnitude < SATISFACTION_RADIUS * SATISFACTION_RADIUS) {
            return new KinematicOutput{orientation = followerInput.orientation};
        }

        velocity /= TIME_TO_TARGET;
        float orientation = GetNewOrientation(followerInput.orientation, velocity);

        return new KinematicOutput{velocity = velocity, orientation = orientation};
    }

    public KinematicOutput PerformWander(FollowerInput followerInput) {
        Vector2 velocity = followerInput.targetPosition - followerInput.position;
        float orientation = followerInput.orientation;
        float rotation = Random.Range(-1f, 1f);

        return new KinematicOutput{velocity = velocity, orientation = orientation, rotation = rotation};
    }
}

public struct KinematicOutput {
    public Vector2 velocity;
    public float orientation;
    public float rotation;
}
