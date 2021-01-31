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
        position = new Vector2(transform.position.x, transform.position.z);
        orientation = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void FixedUpdate() {
        Kinematic kinematic = new Kinematic();
        Vector2 targetPosition = new Vector2(target.position.x, target.position.z);
        FollowerInput followerInput = new FollowerInput(position, targetPosition, orientation);
        KinematicOutput kinematicOutput;
        
        switch (uh) {
            case Uh.Seek:
                kinematicOutput = kinematic.PerformSeek(followerInput);
                break;
            case Uh.Flee:
                kinematicOutput = kinematic.PerformFlee(followerInput);
                break;
            case Uh.Arrive:
                kinematicOutput = kinematic.PerformArrive(followerInput);
                break;
            case Uh.Wander:
                kinematicOutput = kinematic.PerformWander(followerInput);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        position += velocity * Time.deltaTime;
        Debug.Log(kinematicOutput.orientation);
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
