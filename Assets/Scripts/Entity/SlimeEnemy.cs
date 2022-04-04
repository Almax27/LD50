using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : EnemyController
{

    public Grounder grounder;
    public Animator animator;
    public new Rigidbody2D rigidbody2D;

    public float jumpSpeed = 10;

    float nextJumpTime;

    bool pendingJump = false;

    void Awake()
    {
        if (!grounder) grounder = GetComponentInChildren<Grounder>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rigidbody2D) rigidbody2D = GetComponentInChildren<Rigidbody2D>();

        nextJumpTime = Time.time + Random.Range(3, 5);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //isGrounded, onJump, velocityY
        if(animator)
        {
            animator.SetBool("isGrounded", grounder.isGrounded);
            animator.SetFloat("velocityY", rigidbody2D.velocity.y);
        }

        if(nextJumpTime > 0 && Time.time > nextJumpTime)
        {
            print("AttemptJump");

            nextJumpTime = 0;
            animator.SetTrigger("onJump");
        }

        if(pendingJump)
        {
            pendingJump = false;
            rigidbody2D.velocity = new Vector2(0, jumpSpeed);
        }
    }

    //called by PreJump Animation State Behaviour
    void OnPreJumpExit()
    {
        print("OnPreJumpExit");
        pendingJump = true;
    }

    void OnGrounded()
    {
        print("Grounded");
        nextJumpTime = Time.time + Random.Range(3, 5);
    }

}
