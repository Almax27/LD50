using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpositionManager : MonoBehaviour
{
    public string nextLevel = "Level_1";
    public MusicSetup music = new MusicSetup();

    bool canProgress = false;
    bool progress = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Run());

        FAFAudio.Instance.TryPlayMusic(music);
    }

    private void Update()
    {
        if(canProgress && !progress && Input.anyKeyDown)
        {
            progress = true;
            LevelTransition.Instance.TransitionToLevel(nextLevel);
        }
    }

    IEnumerator Run()
    {

        yield return new WaitForSeconds(1.0f);

        canProgress = true;
    }
}
