using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine;

[SelectionBase]
public class Follower : MonoBehaviour {
    public TeamManager TeamManager;
    
    public bool disable;
    public GameObject target;

    [NonSerialized]
    public TeamManager teamManager;

    public bool inMyOwnTerritory { private set; get; } = true;

    public Vector2 position;
    public Vector2 velocity;
    public Vector2 acceleration;
    public float orientation;
    public float rotation;
    public float angularAcceleration;
    [NonSerialized]
    public float maxVelocity = 5f;
    [NonSerialized]
    public float maxRotation = 20f;
    [NonSerialized]
    public float maxAcceleration = 2.5f;

    private Kinematic currentBehavior = new Kinematic();
    public Follower followerTarget { private set; get; }
    public bool hasFollowerScript { private set; get; } = false;

    public bool hasFlag;

    public enum State {
        GetFlag,
        ReturnHome,
        Frozen,
        UnfreezeTeammate,
        FreezeHostile,
        Wander
    }

    // [NonSerialized]
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
                currentBehavior.UpdateTargetHunt(this);
                break;
            case State.ReturnHome:
                currentBehavior.UpdateTargetHunt(this);
                if (inMyOwnTerritory) {
                    SetTarget(null);
                    currentState = State.Wander;
                }
                break;
            case State.Frozen:
                break;
            case State.UnfreezeTeammate:
                currentBehavior.UpdateTargetPursue(this);
                break;
            case State.FreezeHostile:
                currentBehavior.UpdateTargetPursue(this);
                break;
            case State.Wander:
                currentBehavior.UpdateWander(this);
                if (!inMyOwnTerritory) {
                    SetReturnHome();
                } else if (Vector2.Distance(position, Vector2.zero) > 25f) {
                    Vector2 dirToOrigin = -position;
                    // If this position is out of the circle then take a few steps back and reverse your direction.
                    position += dirToOrigin * (Time.deltaTime * 4f);
                    velocity = -velocity;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (!disable) {
            if (hasFlag && inMyOwnTerritory) {
                teamManager.Winner();
            }
        } else {
            position = transform.position.XZ();
        }
    }

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject == teamManager.teamTerritory) {
            inMyOwnTerritory = true;
        }
        
        if (IsState(State.GetFlag)) {
            if (other.gameObject == target) {
                other.transform.parent = transform;
                hasFlag = true;
                SetReturnHome();
            }
        } else if (IsState(State.FreezeHostile)) {
            if (other.gameObject == target && hasFollowerScript) {
                followerTarget.Freeze();
                teamManager.FrozeTarget(followerTarget);
                
                SetTarget(null);
                currentState = State.Wander;
            }
        } else if (IsState(State.UnfreezeTeammate)) {
            if (other.gameObject == target && hasFollowerScript) {
                followerTarget.UnFreeze();
                teamManager.FrozeTarget(followerTarget);
                
                SetReturnHome();
            }
        }
    }

    public void OnTriggerExit(Collider other) {
        if (other.gameObject == teamManager.teamTerritory) {
            inMyOwnTerritory = false;
            teamManager.enemyManager.EnemyInYourTerritory(this);
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

    private void SetReturnHome() {
        currentState = State.ReturnHome;
        SetTarget(teamManager.teamFlag.gameObject);
    }

    private void Freeze() {
        if (hasFlag) {
            teamManager.enemyFlag.FlagReset();
            hasFlag = false;
        }

        teamManager.WasFrozenToday(this);
        SetTarget(null);
        currentState = State.Frozen;
        
    }

    private void UnFreeze() {
        teamManager.FreedTeammate(this);
        SetReturnHome();
    }
}
