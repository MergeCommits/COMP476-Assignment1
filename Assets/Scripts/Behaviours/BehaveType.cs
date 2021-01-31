
using UnityEngine;

public abstract class BehaveType {
    protected const float TIME_TO_TARGET = 1f;
    protected const float SATISFACTION_RADIUS = 0.5f;
    protected const float MAX_PREDICTION = 2.5f;

    protected float RandomBinomial() => Random.value - Random.value;

    protected Vector2 OrientationAsVector(float orientation) =>
        new Vector2(Mathf.Cos(orientation), Mathf.Sin(orientation));
}
