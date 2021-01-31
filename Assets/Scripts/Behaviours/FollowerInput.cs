using UnityEngine;

public class FollowerInput {

    public Vector2 position { get; }
    public Vector2 targetPosition { get; }
    public float orientation { get; }
    
    public FollowerInput(Vector2 position, Vector2 targetPosition, float orientation) {
        this.position = position;
        this.targetPosition = targetPosition;
        this.orientation = orientation;
    }
}
