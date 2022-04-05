using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FAFAudioSFXComponent : MonoBehaviour
{
    public FAFAudioSFXSetup sfx;

    public float delay = 0;
    public float volume = 1;
    public float pitch = 1;

    void Start()
    {
        if (delay <= 0)
        {
            sfx?.Play(transform.position, volume, pitch);
        }
        else
        {
            StartCoroutine(Play_Routine());
        }
    }

    IEnumerator Play_Routine()
    {
        yield return new WaitForSeconds(delay);

        sfx?.Play(transform.position, volume, pitch);
    }
}
