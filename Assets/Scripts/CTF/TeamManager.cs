using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TeamManager : MonoBehaviour {
    private Follower[] followers;

    public String teamName;
    public Flag teamFlag;
    public Flag enemyFlag;
    public GameObject teamTerritory;

    private bool needToSelectNewFlagGetter;

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
    }

    private void ChooseNewFlagGetter() {
        int index = Random.Range(0, followers.Length);
        Follower follower = followers[index];
        if (!follower.IsState(Follower.State.Wander)) {
            return;
        }

        needToSelectNewFlagGetter = false;

        follower.currentState = Follower.State.GetFlag;
        follower.SetTarget(enemyFlag.gameObject);
    }
}
