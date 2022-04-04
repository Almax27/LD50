using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{

    public Transform playerSpawnPoint;
    public PlayerController playerToSpawn;

    public PlayerController currentPlayer;

    SuperTiled2Unity.SuperMap superMap;

    public MusicSetup gameMusic;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        var playerGO = GameObject.Instantiate(playerToSpawn, playerSpawnPoint.position, playerSpawnPoint.rotation);
        currentPlayer = playerGO.GetComponent<PlayerController>();

        var followCamera = GetComponentInChildren<FollowCamera>();
        if(followCamera)
        {
            followCamera.target = playerGO.transform;
        }

        if(gameMusic != null)
        {
            FAFAudio.Instance.TryPlayMusic(gameMusic);
        }
    }

    public SuperTiled2Unity.SuperMap GetMap()
    {
        if (!superMap) superMap = FindObjectOfType<SuperTiled2Unity.SuperMap>();
        return superMap;
    }

    public Vector2 GetMapSize()
    {
        return GetMap() ? new Vector2(GetMap().m_Width, GetMap().m_Height) : Vector2.zero;
    }

    internal void OnLoss(string Message)
    {
        throw new NotImplementedException();
    }
}
