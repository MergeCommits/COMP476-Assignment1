using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine;

public class Follower : MonoBehaviour {
    public TeamManager TeamManager;
    
    public bool disable;
    public GameObject target;

    public Vector2 position;
    public Vector2 velocity;
    public Vector2 acceleration;
    public float orientation;
    public float rotation;
    public float angularAcceleration;
    [NonSerialized]
    public float maxVelocity = 5f;
    [NonSerialized]
    public float maxAcceleration = 2.5f;

    private Kinematic currentBehavior = new Kinematic();
    public Follower followerTarget { private set; get; }
    public bool hasFollowerScript { private set; get; } = false;

    public bool hasFlag;

    public enum State {
        GetFlag,
        Frozen,
        UnfreezeTeammate,
        FreezeHostile,
        Wander
    }

    [NonSerialized]
    public State currentState = State.Wander;

    public bool IsState(State state) => currentState == state;

    private void Start() {
        Transform transform1 = transform;
        
        position = transform1.position.XZ();
        orientation = transform1.rotation.eulerAngles.y;
        if (target != null) {
            followerTarget = target.gameObject.GetComponent<Follower>();
            hasFollowerScript = followerTarget != null;
        }
    }

    void FixedUpdate() {
        switch (currentState) {
            case State.GetFlag:
                if (!hasFlag) {
                    currentBehavior.UpdateTargetHunt(this);
                }
                break;
            case State.Frozen:
                break;
            case State.UnfreezeTeammate:
                break;
            case State.FreezeHostile:
                break;
            case State.Wander:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (!disable) {
            
        } else {
            position = transform.position.XZ();
        }
    }

    public void OnTriggerStay(Collider other) {
        if (IsState(State.GetFlag)) {
            if (other.gameObject == target) {
                
            }
        }
    }

    public void SetTarget(GameObject tar) {
        target = tar;
        if (tar == null) {
            hasFollowerScript = false;
        } else {
            followerTarget = target.gameObject.GetComponent<Follower>();
            hasFollowerScript = followerTarget != null;
        }
    }
}
