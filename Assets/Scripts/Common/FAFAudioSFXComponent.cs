using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FAFAudioSFXComponent : MonoBehaviour
{
    public FAFAudioSFXSetup sfx;

    public float volume = 1;
    public float pitch = 1;

    void Start()
    {
        sfx?.Play(transform.position, volume, pitch);
    }
}
