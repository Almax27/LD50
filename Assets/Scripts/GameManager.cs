using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : SingletonBehaviour<GameManager>
{

    public Transform playerSpawnPoint;
    public PlayerController playerToSpawn;

    public PlayerController currentPlayer;

    public SuperTiled2Unity.SuperMap superMap { get; private set; }

    public SuperTiled2Unity.SuperObjectLayer gameLayer { get; private set; }

    public MusicSetup gameMusic;

    public FAFAudioSFXSetup gameOverSFX;

    bool isRestarting = false;
    public bool isPaused { get; private set; }

    public Text stimText;
    public Image stimBar;
    public CanvasGroup pauseCanvasGroup;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
#if DEBUG
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            var levelEndTrigger = FindObjectOfType<LevelEndTrigger>();
            levelEndTrigger?.CompleteLevel();
        }
#endif

        if(!isRestarting)
        {
            if(!currentPlayer || currentPlayer.GetComponent<Health>().GetIsDead())
            {
                isRestarting = true;
                StartCoroutine(GameOver_Routine());
            }
        }

        if(currentPlayer)
        {
            var health = currentPlayer.GetComponent<Health>();
            if (stimText)
            {
                stimText.enabled = false;
                stimText.text = string.Format("{0}/{1} - {2:0.0}s", health.GetHealth(), health.maxHealth, currentPlayer.sleepTimer);
            }
            if(stimBar)
            {
                Vector3 scale = stimBar.rectTransform.localScale;
                scale.x = 1 - Mathf.Clamp01(currentPlayer.sleepTimer / currentPlayer.timeToSleep);
                stimBar.rectTransform.localScale = scale;

                float timeSinceStim = currentPlayer.TimeSinceLastStim();
                float timeSinceDamage = health.TimeSinceLastDamage();
                Color fromColor = Color.white;
                float tVal = 1;
                if (timeSinceDamage >= 0 && timeSinceDamage < 0.3f)
                {
                    fromColor = Color.red;
                    tVal = timeSinceDamage / 0.3f;
                }
                else if(timeSinceStim >= 0 && timeSinceStim < 0.5f)
                {
                    fromColor = Color.yellow;
                    tVal = timeSinceStim / 0.5f;
                }
                stimBar.color = Color.Lerp(fromColor, Color.white, Mathf.Clamp01(tVal));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
            }
        }

        if(pauseCanvasGroup)
        {
            pauseCanvasGroup.alpha = Mathf.Clamp01(pauseCanvasGroup.alpha + (isPaused ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 5.0f);
        }

    }

    IEnumerator GameOver_Routine()
    {
        gameOverSFX?.Play(Camera.main.transform.position);

        yield return new WaitForSeconds(2.0f);

        LevelTransition.Instance.TransitionToLevel(SceneManager.GetActiveScene().name);
    }
}
