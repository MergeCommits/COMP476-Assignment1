using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using Random = UnityEngine.Random;

public class TeamManager : MonoBehaviour {
    private Follower[] followers;

    public String teamName;
    public Flag teamFlag;
    public Flag enemyFlag;
    public GameObject teamTerritory;
    public TeamManager enemyManager;

    private bool needToSelectNewFlagGetter;

    private List<EnemyInTerritory> enemiesInTerritory = new List<EnemyInTerritory>();
    private List<FrozenTeammate> frozenTeammates = new List<FrozenTeammate>();

    void Start() {
        GameObject[] followerObjects = GameObject.FindGameObjectsWithTag(teamName);
        followers = new Follower[followerObjects.Length];
        for (int i = 0; i < followerObjects.Length; ++i) {
            followers[i] = followerObjects[i].GetComponent<Follower>();
            followers[i].teamManager = this;
            followers[i].currentState = Follower.State.Wander;
        }

        needToSelectNewFlagGetter = true;
    }

    void FixedUpdate() {
        if (needToSelectNewFlagGetter) {
            ChooseNewFlagGetter();
        }

        UpdateEnemiesInTerritory();
        UpdateFreeingTeammates();
    }

    private bool AreScrewed() => followers.All(f => f.IsState(Follower.State.Frozen));

    private void ChooseNewFlagGetter() {
        if (teamName == "Blue") { return; }
        int index = Random.Range(0, followers.Length);
        Follower follower = followers[index];
        if (!follower.IsState(Follower.State.Wander)) {
            return;
        }

        needToSelectNewFlagGetter = false;

        follower.currentState = Follower.State.GetFlag;
        follower.SetTarget(enemyFlag.gameObject);
    }

    #region TerritoryControl
    
    class EnemyInTerritory {
        public Follower enemy;
        public Follower assignedTo = null;
    }

    public void EnemyInYourTerritory(Follower follower) {
        // Can sometimes fire twice from OnTriggerExit, so make sure this agent isn't already in the list.
        bool wut = enemiesInTerritory.Any(e => e.enemy == follower);
        if (!wut) {
            if (follower.gameObject.name == "Follower (2)") { return; }
            enemiesInTerritory.Add(new EnemyInTerritory {enemy = follower, assignedTo = null});
        }
    }

    private void UpdateEnemiesInTerritory() {
        for (int i = 0; i < enemiesInTerritory.Count; i++) {
            EnemyInTerritory inTerritory = enemiesInTerritory[i];
            if (inTerritory.assignedTo != null) {
                // Enemy returned to their own territory -> call off pursue.
                if (inTerritory.enemy.inMyOwnTerritory) {
                    inTerritory.assignedTo.SetTarget(null);
                    inTerritory.assignedTo.currentState = Follower.State.Wander;
                    enemiesInTerritory.RemoveAt(i);
                    i--;
                }
            } else {
                // Enemy returned to their own territory -> remove from list.
                if (inTerritory.enemy.inMyOwnTerritory) {
                    enemiesInTerritory.RemoveAt(i);
                    i--;
                } else {
                    // Assign someone to freeze the enemy.
                    int index = Random.Range(0, followers.Length);
                    Follower follower = followers[index];
                    if (follower.IsState(Follower.State.Wander)) {
                        inTerritory.assignedTo = follower;
                        follower.SetTarget(inTerritory.enemy.gameObject);
                        follower.currentState = Follower.State.FreezeHostile;
                    }
                }
            }
        }
    }

    public void FrozeTarget(Follower target) {
        enemiesInTerritory.Remove(enemiesInTerritory.SingleOrDefault(s => s.enemy == target));
    }
    
    #endregion TerritoryControl

    #region FrozenTeammates

    class FrozenTeammate {
        public Follower frozen;
        public Follower assignedTo = null;
    }

    private void UpdateFreeingTeammates() {
        for (int i = 0; i < frozenTeammates.Count; i++) {
            FrozenTeammate frozenTeammate = frozenTeammates[i];
            if (frozenTeammate.assignedTo == null) {
                // Assign someone to unfreeze them.
                int index = Random.Range(0, followers.Length);
                Follower follower = followers[index];
                if (follower.IsState(Follower.State.Wander)) {
                    frozenTeammate.assignedTo = follower;
                    follower.SetTarget(frozenTeammate.frozen.gameObject);
                    follower.currentState = Follower.State.UnfreezeTeammate;
                }
            }
        }
    }

    public void WasFrozenToday(Follower them) {
        if (them.IsState(Follower.State.GetFlag) || them.hasFlag) {
            // needToSelectNewFlagGetter = true;
        }
        
        frozenTeammates.Add(new FrozenTeammate() {
            frozen = them,
            assignedTo = null
        });
    }

    public void FreedTeammate(Follower them) {
        frozenTeammates.Remove(frozenTeammates.SingleOrDefault(s => s.frozen == them));
    }

    #endregion
}
