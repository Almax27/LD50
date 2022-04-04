using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{

    public Transform playerSpawnPoint;
    public PlayerController playerToSpawn;

    public PlayerController currentPlayer;

    public SuperTiled2Unity.SuperMap superMap { get; private set; }

    public SuperTiled2Unity.SuperObjectLayer gameLayer { get; private set; }

    public MusicSetup gameMusic;

    bool isRestarting = false;

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

        var mapLayers = GetMap().GetComponentsInChildren<SuperTiled2Unity.SuperLayer>();
        foreach(var layer in mapLayers)
        {
            if(layer.m_TiledName == "Game" &&  layer is SuperTiled2Unity.SuperObjectLayer)
            {
                gameLayer = layer as SuperTiled2Unity.SuperObjectLayer;
            }
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

    private void Update()
    {
        if(!isRestarting)
        {
            if(!currentPlayer || currentPlayer.GetComponent<Health>().GetIsDead())
            {
                isRestarting = true;
                StartCoroutine(GameOver_Routine());
            }
        }
    }

    IEnumerator GameOver_Routine()
    {
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
