using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelTransition : SingletonBehaviour<LevelTransition>
{

    public Image image;

    bool inProgress = false;
    string nextLevel;

    float alpha = 0;

    public void TransitionToLevel(string levelName)
    {
        nextLevel = levelName;
        inProgress = true;
    }

        // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(inProgress)
        {
            alpha = Mathf.Clamp01(alpha + Time.deltaTime);

            if (alpha >= 1)
            {
                SceneManager.LoadScene(nextLevel);
                inProgress = false;
            }
        }
        else
        {
            alpha = Mathf.Clamp01(alpha - Time.deltaTime);
        }

        if(image)
        {
            image.enabled = alpha > 0;
            image.color = new Color(0, 0, 0, alpha);
        }
    }
}
