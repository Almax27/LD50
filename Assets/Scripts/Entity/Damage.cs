using UnityEngine;
using System.Collections;

public class Damage {

    public Damage(float damage, GameObject sender)
    {
        value = damage;
        owner = sender;
    }
    public float value;
    public GameObject owner;
}
