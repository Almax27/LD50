using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public string firstLevelName = "Level_1";
    public Animator titleAnimator;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunTitleScreen());
    }

    IEnumerator RunTitleScreen()
    {
        yield return new WaitForSeconds(1.0f);

        titleAnimator.SetTrigger("play");

        yield return new WaitForSeconds(3.0f);

        SceneManager.LoadScene(firstLevelName);
    }
}
