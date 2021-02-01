using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour {
    public bool disable;
    public Follower target;

    public Vector2 position;
    public Vector2 velocity;
    public Vector2 acceleration;
    public float orientation;
    public float rotation;
    public float angularAcceleration;

    public float maxVelocity = 10f;
    public float maxAcceleration = 5f;

    private Kinematic currentBehavior = new Kinematic();

    private void Start() {
        Transform transform1 = transform;
        Vector3 position1 = transform1.position;
        
        position = new Vector2(position1.x, position1.z);
        orientation = transform1.rotation.eulerAngles.y;
    }

    void FixedUpdate() {
        if (!disable) {
            currentBehavior.UpdateTargetHunt(this);
        } else {
            position = new Vector2(transform.position.x, transform.position.z);
        }
    }
}
