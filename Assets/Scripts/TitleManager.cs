using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public string firstLevelName = "Level_1";
    public Animator titleAnimator;
    public MusicSetup music;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunTitleScreen());

        FAFAudio.Instance.TryPlayMusic(music);
    }

    IEnumerator RunTitleScreen()
    {
        yield return new WaitForSeconds(1.0f);

        titleAnimator.SetTrigger("play");

        yield return new WaitForSeconds(3.0f);

        SceneManager.LoadScene(firstLevelName);
    }
}
