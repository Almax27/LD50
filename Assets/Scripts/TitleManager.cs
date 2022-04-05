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

    public Text text;

    bool progress = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunTitleScreen());

        FAFAudio.Instance.TryPlayMusic(music);
    }

    private void Update()
    {
        if(Input.anyKeyDown)
        {
            progress = true;
        }
    }


    IEnumerator RunTitleScreen()
    {
        yield return new WaitForSeconds(1.0f);

        titleAnimator.SetTrigger("play");

        while(!progress)
        {
            yield return new WaitForSeconds(0.1f);
        }

        SceneManager.LoadScene(firstLevelName);
    }
}
