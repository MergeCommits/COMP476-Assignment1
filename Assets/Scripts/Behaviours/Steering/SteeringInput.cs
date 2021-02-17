using UnityEngine;

public struct SteeringInput {
    public Vector2 position;
    public Vector2 velocity;
    public float orientation;
    public float rotation;
    public float maxVelocity;
    public float maxRotation;
    public float maxAcceleration;
    public float maxAngularAcceleration;
    public Vector2 targetPosition;
    public Vector2 targetVelocity;
    public float targetOrientation;
}
