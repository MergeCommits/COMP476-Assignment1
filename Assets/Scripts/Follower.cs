using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour {
    // Start is called before the first frame update
    public enum Uh {
        Seek,
        Flee,
        Arrive,
        Wander
    }

    public Uh uh;
    public Transform target;

    private Vector2 position;
    private Vector2 velocity;
    private float orientation;
    private float rotation;

    private float maxVelocity = 10f;

    private void Start() {
        Transform transform1 = transform;
        Vector3 position1 = transform1.position;
        
        position = new Vector2(position1.x, position1.z);
        orientation = transform1.rotation.eulerAngles.y;
    }

    void FixedUpdate() {
        Kinematic kinematic = new Kinematic();
        
        Vector3 targetPosition = target.position;
        KinematicInput followerInput = new KinematicInput {
            position = position,
            velocity = velocity,
            targetPosition = new Vector2(targetPosition.x, targetPosition.z),
            orientation = orientation
        };

        KinematicOutput kinematicOutput = uh switch {
            Uh.Seek => kinematic.PerformSeek(followerInput),
            Uh.Flee => kinematic.PerformFlee(followerInput),
            Uh.Arrive => kinematic.PerformArrive(followerInput),
            Uh.Wander => kinematic.PerformWander(followerInput),
            _ => throw new ArgumentOutOfRangeException()
        };

        position += velocity * Time.deltaTime;
        orientation = kinematicOutput.orientation;
        orientation += rotation * Time.deltaTime;

        Transform transform1 = transform;
        transform1.position = new Vector3(position.x, transform1.position.y, position.y);
        transform1.eulerAngles = new Vector3(0f, -orientation * Mathf.Rad2Deg, 0f);
        
        velocity = kinematicOutput.velocity;
        if (velocity.sqrMagnitude > maxVelocity * maxVelocity) {
            velocity = velocity.normalized;
            velocity *= maxVelocity;
        }
        rotation = kinematicOutput.rotation;
    }
}
