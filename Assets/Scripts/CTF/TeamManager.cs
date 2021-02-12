using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    void Start() {
        GameObject[] followerObjects = GameObject.FindGameObjectsWithTag(teamName);
        followers = new Follower[followerObjects.Length];
        for (int i = 0; i < followerObjects.Length; ++i) {
            followers[i] = followerObjects[i].GetComponent<Follower>();
            followers[i].teamManager = this;
        }

        needToSelectNewFlagGetter = true;
    }

    void FixedUpdate() {
        if (needToSelectNewFlagGetter) {
            ChooseNewFlagGetter();
        }

        UpdateEnemiesInTerritory();
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
    
    class EnemyInTerritory {
        public Follower enemy;
        public Follower assignedTo = null;
    }

    public void EnemyInYourTerritory(Follower follower) =>
        enemiesInTerritory.Add(new EnemyInTerritory {
            enemy = follower,
            assignedTo = null
        });

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

    public void FlagGetterFrozen() {
        needToSelectNewFlagGetter = true;
    }
}
