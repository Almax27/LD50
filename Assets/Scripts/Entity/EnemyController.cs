using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Knockback")]
    public float stunDuration;
    public Vector2 knockbackVector = new Vector2(7,5);

    protected float stunnedUntilTime;
    protected float lastDamageTime;

    protected Vector2 desiredVelocity;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StunFor(float duration)
    {
        stunnedUntilTime = Time.time + duration;
    }

    public bool IsStunned()
    {
        return Time.time < stunnedUntilTime;
    }

    public virtual void OnDamage(Damage damage)
    {
        lastDamageTime = Time.time;

        if (damage.owner)
        {
            Vector2 knockbackDir = (transform.position - damage.owner.transform.position).normalized;
            desiredVelocity.x = Mathf.Sign(knockbackDir.x) * damage.knockback.x;
            desiredVelocity.y = damage.knockback.y;
            GetComponent<Rigidbody2D>().velocity = desiredVelocity;
            StunFor(stunDuration);
        }
        //animator.SetTrigger("onDamage");
    }
}
