using UnityEngine;
using System.Collections;

public class Damage {

    public Damage(float damage, GameObject sender, FAFAudioSFXSetup hitSFXSetup = null)
    {
        value = damage;
        owner = sender;
        hitSFX = hitSFXSetup;
    }
    public float value;
    public GameObject owner;
    public FAFAudioSFXSetup hitSFX;
}
