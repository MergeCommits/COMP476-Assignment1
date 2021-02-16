
using UnityEngine;

public abstract class BehaveType {
    protected const float TIME_TO_TARGET = 1f;
    protected const float SATISFACTION_RADIUS = 0.5f;
    protected const float MAX_PREDICTION = 0.5f;
    protected const float SLOW_RADIUS = 4f;
    protected const float SLOW_SPEED = 1f;

    protected float RandomBinomial() => Random.value - Random.value;

    protected Vector2 OrientationAsVector(float orientation) =>
        new Vector2(Mathf.Cos(orientation), Mathf.Sin(orientation));

    protected bool AngleDifferenceNegligible(float currentOrientation, float targetOrientation, float tolerance) {
        float angleDifferenceBetween = Mathf.DeltaAngle(currentOrientation * Mathf.Rad2Deg, targetOrientation * Mathf.Rad2Deg);
        return Mathf.Abs(angleDifferenceBetween) < tolerance;
    }
}
