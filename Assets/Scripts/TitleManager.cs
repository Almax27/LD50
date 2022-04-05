using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public string firstLevelName = "Level_1";
    public Animator titleAnimator;
    public MusicSetup music;

    public FAFAudioSFXSetup onStartSFX;

    public Text text;

    bool canProgress = false;
    bool progress = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunTitleScreen());

        FAFAudio.Instance.TryPlayMusic(music);

        if (text)
        {
            text.enabled = false;
            text.color = new Color(1, 1, 1, 0);
        }
    }

    private void Update()
    {
        if(!progress && canProgress && Input.anyKeyDown)
        {
            progress = true;
            onStartSFX?.Play(Camera.main.transform.position);
        }
        if (text && text.enabled)
        {
            Color c = text.color;
            c.a = Mathf.Clamp01(c.a + Time.deltaTime);
            text.color = c;
        }
    }


    IEnumerator RunTitleScreen()
    {
        yield return new WaitForSeconds(1.0f);

        titleAnimator.SetTrigger("play");

        yield return new WaitForSeconds(3.0f);

        canProgress = true;

        if (text) text.enabled = true;

        while (!progress)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (text) text.enabled = false;

        yield return new WaitForSeconds(1.0f);

        LevelTransition.Instance.TransitionToLevel(firstLevelName);
    }
}
