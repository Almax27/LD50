using UnityEngine;
using System.Collections.Generic;

public class Damage {

    public Damage(float damage, GameObject sender, FAFAudioSFXSetup hitSFXSetup = null, Vector2 knockbackVector = new Vector2())
    {
        value = damage;
        owner = sender;
        hitSFX = hitSFXSetup;
        knockback = knockbackVector;

        hitObjects = new List<GameObject>();
    }
    public float value;
    public GameObject owner;
    public FAFAudioSFXSetup hitSFX;
    public Vector2 knockback;
    public bool consumed;

    public List<GameObject> hitObjects { get; private set; }
}
