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
    public LayerMask worldCollisionMask;

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

        var playerGO = GameObject.Instantiate(playerToSpawn, GetPlayerSpawnLocation(), Quaternion.identity);
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

    Vector3 GetPlayerSpawnLocation()
    {
        Vector2 loc = Vector2.zero;
        if (playerSpawnPoint)
        {
            loc = playerSpawnPoint.position;
        }
        if (playerToSpawn)
        {
            var playerCapsule = playerToSpawn.GetComponent<CapsuleCollider2D>();
            if(playerCapsule)
            {
                Vector2 checkDir = Vector2.down;
                float checkDist = 2.0f;
                var hit = Physics2D.CapsuleCast(loc, playerCapsule.size, playerCapsule.direction, 0, checkDir, checkDist);
                if(hit)
                {
                    loc += checkDir * checkDist * hit.fraction;
                }
            }
        }
        return loc;
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

    public Rect GetMapBounds(Vector2 inset = default)
    {
        var mapSize = GetMapSize();
        return new Rect(inset.x, inset.y - mapSize.y, mapSize.x - inset.x, mapSize.y - inset.y);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            //LevelTransition.Instance.TransitionToLevel(SceneManager.GetActiveScene().name);
            if (currentPlayer) currentPlayer.transform.position = GetPlayerSpawnLocation();
        }
#endif

        if (!isRestarting)
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
                stimBar.enabled = true;

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
        else
        {
            if(stimBar) stimBar.enabled = false;
        }

        if (Input.GetButtonDown("Pause"))
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
