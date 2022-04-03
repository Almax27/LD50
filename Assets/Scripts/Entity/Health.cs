using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public float maxHealth = 10;
    public float currentHealth = 10;

    public GameObject[] spawnOnDamage = new GameObject[0];
    public GameObject[] spawnOnDeath = new GameObject[0];

    public Color damageTintColor = Color.red;
    public float damageTintDuration = 0.2f;

    bool isDead = false;

    float lastDamageTime = 0;
    bool isDamageTinted = false;

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
        lastDamageTime = damageTintDuration;
    }

    void OnDamage(Damage damage)
    {
        currentHealth -= damage.value;

        foreach (GameObject gobj in spawnOnDamage)
        {
            Instantiate(gobj, this.transform.position, this.transform.rotation);
        }

        if (!isDead && currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            SendMessage("OnDeath");
        }

        lastDamageTime = Time.time;
    }

    void OnDeath()
    {
        foreach (GameObject gobj in spawnOnDeath)
        {
            Instantiate(gobj, this.transform.position, this.transform.rotation);
        }
    }

    void Update()
    {
        bool doTint = Time.time - lastDamageTime < damageTintDuration;
        if(doTint != isDamageTinted)
        {
            isDamageTinted = doTint;
            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.color = isDamageTinted ? damageTintColor : Color.white;
            }
        }
    }
}