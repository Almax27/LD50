using UnityEngine;
using System.Collections;

public class Damage {

    public Damage(float damage, GameObject sender, FAFAudioSFXSetup hitSFXSetup = null, Vector2 knockbackVector = new Vector2())
    {
        value = damage;
        owner = sender;
        hitSFX = hitSFXSetup;
        knockback = knockbackVector;
    }
    public float value;
    public GameObject owner;
    public FAFAudioSFXSetup hitSFX;
    public Vector2 knockback;
    public bool consumed;
}
