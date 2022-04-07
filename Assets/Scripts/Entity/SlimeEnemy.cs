using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : EnemyController
{
    public float jumpSpeed = 10;

    public float aggroRange = 10;

    public float spikeAttackDamage = 2.0f;
    public Vector2 spikeAttackKnockback = new Vector2(8, 3);

    float nextActionTime;

    SpriteRenderer sprite;
    AttackBox attackBox;

    Damage attackDamage;

    protected override void Awake()
    {
        base.Awake();

        sprite = GetComponentInChildren<SpriteRenderer>();
        attackBox = GetComponentInChildren<AttackBox>();

        ResetAction();
    }

    // Update is called once per frame
    void Update()
    {
        //isGrounded, onJump, velocityY
        if(animator)
        {
            animator.SetBool("isGrounded", grounder.IsGrounded(0));
            animator.SetFloat("velocityY", rigidbody2D.velocity.y);

            if (grounder.IsGrounded())
            {
                if (nextActionTime > 0 && Time.time > nextActionTime)
                {
                    //print("AttemptJump");
                    nextActionTime = 0;
                    if (Random.value > 0.5f)
                    {
                        animator.SetTrigger("onJump");
                    }
                    else
                    {
                        animator.SetTrigger("onAttack");
                    }
                }
                //sprite.transform.rotation = Quaternion.identity;
            }
            else
            {
                //sprite.transform.rotation = Quaternion.Euler(0, 0, Vector2.Angle(rigidbody2D.velocity, Vector2.up));
            }
        }

        if (attackBox && attackDamage != null)
        {
            attackBox.DealDamage(attackDamage);
        }
    }

    void ResetAction()
    {
        nextActionTime = Time.time + Random.Range(2, 4);
    }

    void DoJump()
    {
        if (grounder.IsGrounded(0))
        {
            Vector2 jumpDirection = new Vector2(0, 1);

            var player = GameManager.Instance.currentPlayer;
            if (player)
            {
                Vector2 playerVector = player.transform.position - transform.position;
                if (playerVector.sqrMagnitude < aggroRange * aggroRange)
                {
                    var hit = Physics2D.Raycast(transform.position, playerVector.normalized, playerVector.magnitude, grounder.groundMask);
                    if (hit.transform == null)
                    {
                        jumpDirection.x = Mathf.Sign(player.transform.position.x - transform.position.x);
                    }
                }
            }
            rigidbody2D.velocity = jumpDirection.normalized * jumpSpeed;
        }

        ResetAction();
    }

    void StartAttack()
    {
        attackDamage = new Damage(spikeAttackDamage, gameObject, null, spikeAttackKnockback);
    }

    void EndAttack()
    {
        attackDamage = null;
        ResetAction();
    }

    void OnGrounded()
    {
        //print("Grounded");
        
    }

    public override void OnDamage(Damage damage)
    {
        base.OnDamage(damage);
        ResetAction();
    }

    public void OnAttackHit(Damage damage)
    {
        rigidbody2D.velocity = -rigidbody2D.velocity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

}
