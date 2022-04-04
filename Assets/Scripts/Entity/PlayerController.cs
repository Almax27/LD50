using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class MeleeAttack
{
    public float damage;
    public float damageDelay = 0.3f;

    public float playerSpeedX = 10.0f;

    public Vector2 knockback;

    public FAFAudioSFXSetup attackSound;
    public FAFAudioSFXSetup hitSound;
}

public class Idle
{
    public float weight = 0;
    public AnimationClip anim = null;
}


public class PlayerController : MonoBehaviour
{

    public SpriteRenderer body;
    public Grounder grounder;
    public Climber climber;
    public Grounder climbGrounder;

    [Header("Movement")]
    public float groundSpeed = 12.0f;
    public float airSpeed = 6.0f;
    public float idleSpeed = 5.0f;
    public float climbingSpeed = 0.2f;
    public bool allowMoveToCancelClimb = false;
    public PhysicsMaterial2D groundedPhysicsMaterial;
    public PhysicsMaterial2D airPhysicsMaterial;
    public float footstepInterval = 0.5f;
    public FAFAudioSFXSetup footStepAudio;
    public FAFAudioSFXSetup jumpAudio;
    public FAFAudioSFXSetup landAudio;

    [Header("Jumping")]
    public float gravity = 1;
    public AnimationCurve jumpCurve = new AnimationCurve();
    public float jumpHeight = 1;
    public float minJumpTime = 0.1f;
    public float maxJumpTime = 0.3f;
    public GameObject[] spawnOnJump = new GameObject[0];

    [Header("Climbing")]
    public float climbExitTime = 0.2f; //time after leaving climbable before can climb again

    [Header("MeleeAttack")]
    public AttackBox meleeAttackBox;
    public MeleeAttack[] meleeAttacks = new MeleeAttack[0];

    [Header("Damage")]
    public float stunDuration = 0.2f;
    public Vector2 knockbackVector = new Vector2(5.0f, 3.0f);

    [Header("Sleeping")]
    public bool isSleeping = false;
    public int pressesToWakeUp = 10;
    bool canRecoverFromSleeping = false;
    int wakePresses = 0;

    [Header("State")]
    public bool isMovingRight = true;
    public bool isLookingRight = true;
    public bool isClimbing = false;

    Animation bodyAnim;
    Animator animator;
    new Rigidbody2D rigidbody2D;
    CapsuleCollider2D capsuleCollider2D;

    float footstepDistTraveled = 0;

    int jumpsRemaining = 0;
    int maxJumps = 1;
    bool isJumping = false;
    float jumpTick = 0;

    bool hasMovedInAir = false;

    bool wantsJump = false;
    bool jumpPendingRelease = false;
    float xInputRaw = 0;
    float xInput = 0;
    float yInput = 0;

    bool isAttacking = false;
    bool pendingAttackDamage = false;
    float lastAttackTime = 0;
    int attackComboIndex = 0;

    float lastClimbTime = 0;
    float lastDamageTime = 0;

    Vector2 lastPosition;
    Vector2 desiredVelocity;
    Vector2 acceleration;

    float stunnedUntilTime;

    void Start()
    {
        if (!meleeAttackBox) meleeAttackBox = GetComponentInChildren<AttackBox>();
        bodyAnim = body ? body.GetComponent<Animation>() : null;
        animator = GetComponentInChildren<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        isAttacking = lastAttackTime > 0;

        //get input
        xInputRaw = Input.GetAxisRaw("Horizontal");
        xInput = xInputRaw; // Mathf.SmoothDamp(xInput, xInputRaw, ref xInputVel, 0.1f, float.MaxValue, Time.deltaTime);
        yInput = Input.GetAxis("Vertical");

        wantsJump = Input.GetButton("Jump");
        if (jumpPendingRelease && !wantsJump) jumpPendingRelease = false;

        if (Input.GetKeyDown(KeyCode.S))
        {
            if(isSleeping && canRecoverFromSleeping)
            {
                wakePresses++;
                print(string.Format("Pressed {0}/{1} to wake up", wakePresses, pressesToWakeUp));
                if (wakePresses >= pressesToWakeUp)
                {
                    WakeUp();
                }
            }
            else
            {
                Sleep();
            }
        }

        if (CanAttack() && Input.GetButtonDown("Attack"))
        {
           // if(isAttacking) attackComboIndex++;

            animator.SetTrigger("onAttack");
            lastAttackTime = Time.time;
            pendingAttackDamage = true;

            if (xInputRaw != 0) isLookingRight = xInputRaw > 0;

            var currentAttack = GetCurrentAttack();
            if (meleeAttackBox && currentAttack != null)
            {
                if (meleeAttackBox)
                {
                    float attackMoveSpeed = currentAttack.playerSpeedX * (meleeAttackBox.IsTargetInRange() ? 0.5f : 1.0f);
                    desiredVelocity = new Vector2(isLookingRight ? Mathf.Max(desiredVelocity.x, attackMoveSpeed) : Mathf.Min(desiredVelocity.x, -attackMoveSpeed), desiredVelocity.y);
                }

                currentAttack.attackSound?.Play(transform.position);
                meleeAttackBox.DealDamage(new Damage(currentAttack.damage, gameObject, currentAttack.hitSound, currentAttack.knockback), currentAttack.damageDelay);
            }
        }

        HandleAttacking();

        bool isMoving = Mathf.Abs(rigidbody2D.velocity.x) > idleSpeed;

        animator.SetFloat("moveSpeed", isMoving ? Mathf.Abs(rigidbody2D.velocity.x / groundSpeed) : 0);
        animator.SetFloat("velocityY", desiredVelocity.y);
        animator.SetBool("isIdle", !isMoving && !isAttacking);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isSleeping", isSleeping);
    }

    void FixedUpdate()
    {
        var isGrounded = grounder ? grounder.isGrounded : false;
        var isClimbGrounded = climbGrounder ? climbGrounder.isGrounded : false;

        //apply gravity
        if (isGrounded)
        {
            desiredVelocity.y = 0;
        }
        else if (!isClimbing && !isJumping)
        {
            float gravityMultipler = 1.0f;
            if(pendingAttackDamage && attackComboIndex == 0 && desiredVelocity.y < 0)
            {
                gravityMultipler = 0;
            }
            desiredVelocity.y -= gravity * Time.fixedDeltaTime * gravityMultipler;
        }

        capsuleCollider2D.sharedMaterial = isGrounded ? groundedPhysicsMaterial : airPhysicsMaterial;

        HandleClimbing(isGrounded, isClimbGrounded);
        HandleJumping(isGrounded);
        HandleMovement(isGrounded);

        UpdateFacing();

        //update animator
        if (animator && animator.runtimeAnimatorController)
        {
            animator.SetBool("isGrounded", isGrounded);
            //animator.SetBool("isClimbing", isClimbing);
        }

        //move rigidbody
        rigidbody2D.isKinematic = isClimbing;

        if (isGrounded)
        {
            float deltaX = Mathf.Abs(rigidbody2D.position.x - lastPosition.x);
            footstepDistTraveled += deltaX;
            if (footstepDistTraveled > footstepInterval)
            {
                footstepDistTraveled -= footstepInterval;
                footStepAudio.Play(transform.position);
            }
            else if(deltaX < 0.01f)
            {
                footstepDistTraveled = footstepInterval * 0.99f;
            }
        }
        else
        {
            footstepDistTraveled = footstepInterval * 0.99f;
        }

        lastPosition = rigidbody2D.position;
        rigidbody2D.velocity = desiredVelocity;
    }

    MeleeAttack GetCurrentAttack()
    {
        if (meleeAttacks.Length > 0 && attackComboIndex < meleeAttacks.Length)
        {
            return meleeAttacks[attackComboIndex];
        }
        return null;
    }

    bool CanAttack()
    {
        return !isSleeping && !pendingAttackDamage && attackComboIndex < 3 && !IsStunned();
    }

    void HandleAttacking()
    {
        if (lastAttackTime > 0)
        {
            float attackTime = Time.time - lastAttackTime;

            //Reset attack
            if (attackTime > 0.6f) 
            {
                lastAttackTime = 0;
                attackComboIndex = 0;
                pendingAttackDamage = false;
            }
            //Trigger damage
            else if (attackTime > 0.3f)
            {
                if (pendingAttackDamage)
                {
                    animator.ResetTrigger("onAttack");
                    pendingAttackDamage = false;
                    attackComboIndex++;
                }
            }
        }
    }

    void HandleClimbing(bool isGrounded, bool isClimbingGrounded)
    {
        if (climber == null) return;

        //handle climbing
        bool wasClimbing = isClimbing;
        isClimbing |= yInput != 0 && (Time.time - lastClimbTime > climbExitTime);
        isClimbing &= climber.canClimb;

        //handle move exit case
        if (allowMoveToCancelClimb && xInputRaw != 0)
        {
            isClimbing = false;
        }
        //handle jump exit case
        else if (xInputRaw != 0 && wantsJump)
        {
            isClimbing = false;
        }
        //handle botom of ladder case
        else if (isGrounded && isClimbingGrounded && yInput < 0 && !climber.atTopOfLadder)
        {
            isClimbing = false;
        }
        //handle top of ladder case
        else if (isGrounded && yInput > 0 && climber.atTopOfLadder)
        {
            isClimbing = false;
        }

        if (isClimbing)
        {
            //move climbing
            desiredVelocity.y = yInput * climbingSpeed;

            //snap to ladder
            var pos = transform.position;
            pos.x = climber.lockX;
            transform.position = pos;

            wantsJump = false;
            lastClimbTime = Time.time;
        }
        if (wasClimbing != isClimbing)
        {
            desiredVelocity.y = 0;
        }
    }

    bool CanJump()
    {
        return (!isAttacking || attackComboIndex == 0) && !isSleeping && !IsStunned();
    }

    void HandleJumping(bool isGrounded)
    {
        //Reset jump
        if (!wantsJump && (isGrounded || isClimbing))
        {
            jumpsRemaining = maxJumps;
            if(isJumping)
            {
                print("Jump Reset");
                isJumping = false;
            }
        }

        if (isJumping)
        {
            //Cancel jump
            if (!wantsJump && jumpTick >= minJumpTime)
            {
                print("Jump Canceled");
                isJumping = false;
            }
            //End jump
            if (jumpTick >= maxJumpTime)
            {
                print("Jump Ended");
                isJumping = false;
                desiredVelocity.y = 0; //enforce jump height
            }
        }

        //Start jump
        if (CanJump() && !isJumping && wantsJump && !jumpPendingRelease && jumpsRemaining > 0)
        {
            print("Jump Started");
            jumpTick = 0;
            isJumping = true;
            jumpsRemaining--;
            jumpPendingRelease = true; //latch the input
            OnJump();
        }
        
        //Process jump
        if (isJumping)
        {
            print("Jumping...");
            float lastJumpTick = jumpTick;
            jumpTick = Mathf.Clamp(jumpTick + Time.fixedDeltaTime, 0, maxJumpTime);
            float deltaY = (jumpCurve.Evaluate(jumpTick / maxJumpTime) - jumpCurve.Evaluate(lastJumpTick / maxJumpTime)) * jumpHeight;
            desiredVelocity.y = deltaY / Time.fixedDeltaTime;
        }
    }

    bool CanMove()
    {
        return !isSleeping && !isClimbing && !isAttacking && !IsStunned();
    }

    void HandleMovement(bool isGrounded)
    {
        //handle movement
        if (isGrounded)
        {
            hasMovedInAir = false;
        }

        if(IsStunned())
        {
            desiredVelocity = rigidbody2D.velocity;
        }
        else if (CanMove())
        {
            float desiredSpeed = groundSpeed;

#if DEBUG
            if (Input.GetKey(KeyCode.LeftShift)) desiredSpeed *= 3.0f;
#endif

            //use air speed when changing direction in the air
            if (!isGrounded && (hasMovedInAir || Mathf.Sign(desiredVelocity.x) != Mathf.Sign(xInput)))
            {
                desiredSpeed = airSpeed;
                hasMovedInAir = true;
            }

            Vector2 desiredInputVelocity = new Vector2(xInput * desiredSpeed, isGrounded ? 0 : desiredVelocity.y);

            var groundHit = grounder.GetGroundHit();
            if (groundHit && xInput != 0)
            {
                float angleUp = Vector2.Angle(groundHit.normal, Vector2.up);

                if (angleUp < 60)
                {
                    Vector2 tangent = Vector2.Perpendicular(groundHit.normal);

                    tangent = tangent.normalized * -xInput;

                    desiredInputVelocity = tangent * desiredSpeed;
                }
                Debug.DrawLine(transform.position, transform.position + new Vector3(desiredInputVelocity.x, desiredInputVelocity.y, 0));
            }

            
            if (xInput != 0)
            {
                desiredVelocity = Vector2.SmoothDamp(desiredVelocity, desiredInputVelocity, ref acceleration, 0.2f, float.MaxValue, Time.fixedDeltaTime);
            }
            else
            {
                desiredVelocity = Vector2.SmoothDamp(desiredVelocity, desiredInputVelocity, ref acceleration, 0.15f, float.MaxValue, Time.fixedDeltaTime);
            }
        }
        else
        {
            xInput = 0;
            desiredVelocity = Vector2.SmoothDamp(desiredVelocity, new Vector2(0, desiredVelocity.y), ref acceleration, isAttacking ? 0.2f : 0.05f, float.MaxValue, Time.fixedDeltaTime);
        }
    }

    bool CanLook()
    {
        return !isSleeping && !isAttacking && !IsStunned();
    }

    void UpdateFacing()
    {
        if (Mathf.Abs(xInput) > 0.0001f)
        {
            isMovingRight = xInput > 0;
        }
        if (CanLook() && Mathf.Abs(xInputRaw) > 0.0001f)
        {
            isLookingRight = xInputRaw > 0;
        }
        if (body)
        {
            body.flipX = !isLookingRight;

            var p = body.transform.localPosition;
            p.x = Mathf.Abs(p.x) * (isLookingRight ? 1 : -1);
            body.transform.localPosition = p;
        }

        if (meleeAttackBox)
        {
            var p = meleeAttackBox.transform.localPosition;
            p.x = Mathf.Abs(p.x) * (isLookingRight ? 1 : -1);
            meleeAttackBox.transform.localPosition = p;
        }
    }

    void OnJump()
    {
        foreach (var prefab in spawnOnJump)
        {
            if (prefab)
            {
                var gobj = Instantiate(prefab);
                gobj.transform.position = transform.position;
            }
        }

        jumpAudio?.Play(transform.position);
    }

    void OnGrounded()
    {
        landAudio?.Play(transform.position);
        desiredVelocity.y = 0;
        rigidbody2D.velocity = desiredVelocity;
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
            Vector2 knockback = (transform.position - damage.owner.transform.position).normalized;
            desiredVelocity.x = Mathf.Sign(knockback.x) * knockbackVector.x;
            desiredVelocity.y = knockbackVector.y;
            rigidbody2D.velocity = desiredVelocity;
            StunFor(stunDuration);
        }
        //animator.SetTrigger("onDamage");
    }

    void OnDeath()
    {
        Sleep();

        //this.enabled = false;
        //FindObjectOfType<GameManager>().OnLoss("You Died.");
    }

    void OnDeathAnimFinished()
    {
        //Destroy(gameObject);
    }

    void OnFallingAsleepExit()
    {
        canRecoverFromSleeping = true;
    }

    void Sleep()
    {
        if (!isSleeping)
        {
            isSleeping = true;
            animator.SetTrigger("onSleep");
        }
    }

    void WakeUp()
    {
        isSleeping = false;
        wakePresses = 0;
        canRecoverFromSleeping = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach(var contact in collision.contacts)
        {
            float angle = Vector2.Angle(contact.normal, Vector2.up);
            if (angle < 90) //we hit our head on something :(
            {
                isJumping = false;
                desiredVelocity.y = 0;
            }
            else if(angle > 60)
            {
                desiredVelocity.y = 0;
            }
        }
    }
}
