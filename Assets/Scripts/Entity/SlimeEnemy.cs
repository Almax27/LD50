using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : EnemyController
{
    public float jumpSpeed = 10;

    public float aggroRange = 10;

    float nextJumpTime;

    bool pendingJump = false;

    protected override void Awake()
    {
        base.Awake();
        ResetJump();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //isGrounded, onJump, velocityY
        if(animator)
        {
            animator.SetBool("isGrounded", grounder.IsGrounded());
            animator.SetFloat("velocityY", rigidbody2D.velocity.y);
        }

        if(grounder.IsGrounded() && nextJumpTime > 0 && Time.time > nextJumpTime)
        {
            //print("AttemptJump");
            nextJumpTime = 0;
            animator.SetTrigger("onJump");
        }

        if(pendingJump)
        {
            pendingJump = false;

            if (grounder.IsGrounded(0))
            {
                Vector2 jumpDirection = new Vector2(0, 2);

                var player = GameManager.Instance.currentPlayer;
                if (player)
                {
                    Vector2 playerVector = player.transform.position - transform.position;
                    if (playerVector.sqrMagnitude < aggroRange * aggroRange)
                    {
                        var hit = Physics2D.Raycast(transform.position, playerVector.normalized, aggroRange, grounder.groundMask);
                        if (hit.transform == null)
                        {
                            jumpDirection.x = Mathf.Sign(player.transform.position.x - transform.position.x);
                        }
                    }
                }
                rigidbody2D.velocity = jumpDirection.normalized * jumpSpeed;
            }

            ResetJump();
        }
    }

    void ResetJump()
    {
        nextJumpTime = Time.time + Random.Range(3, 5);
    }

    //called by PreJump Animation State Behaviour
    void OnPreJumpExit()
    {
       // print("OnPreJumpExit");
        pendingJump = true;
    }

    void OnGrounded()
    {
        //print("Grounded");
        
    }

    public override void OnDamage(Damage damage)
    {
        base.OnDamage(damage);
        ResetJump();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

}
