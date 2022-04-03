using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public Transform playerSpawnPoint;
    public PlayerController playerToSpawn;

    PlayerController currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        var playerGO = GameObject.Instantiate(playerToSpawn, playerSpawnPoint.position, playerSpawnPoint.rotation);
        currentPlayer = playerGO.GetComponent<PlayerController>();

        var followCamera = GetComponentInChildren<FollowCamera>();
        if(followCamera)
        {
            followCamera.target = playerGO.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void OnLoss(string Message)
    {
        throw new NotImplementedException();
    }
}
