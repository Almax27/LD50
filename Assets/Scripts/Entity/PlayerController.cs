using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class MeleeAttack
{
    public float damage;
    public float damageDelay = 0.3f;
    public float nextAttackDelay = 0.3f;

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
    public float gravity = 1;
    public float groundSpeed = 12.0f;
    public float airSpeed = 6.0f;
    public float idleSpeed = 1.0f;
    public float climbingSpeed = 0.2f;
    public float maxFallingSpeedY = 20;
    public float maxFloorSlopeAngle = 60;
    public bool allowMoveToCancelClimb = false;
    public PhysicsMaterial2D groundedPhysicsMaterial;
    public PhysicsMaterial2D airPhysicsMaterial;
    public float footstepInterval = 0.5f;
    public FAFAudioSFXSetup footStepAudio;
    public FAFAudioSFXSetup jumpAudio;
    public FAFAudioSFXSetup landAudio;

    [Header("Jumping")]
    public int maxJumps = 1;
    public float jumpHeight = 1;    
    public float minJumpTime = 0.1f;
    public float maxJumpTime = 0.3f;
    public AnimationCurve jumpCurve = new AnimationCurve();
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
    public float timeToSleep = 10;
    public bool isSleeping = false;
    public int pressesToWakeUp = 10;
    public float sleepTimer = 0;
    public float lastStimTime = 0;

    [Header("State")]
    public bool isMovingRight = true;
    public bool isLookingRight = true;
    public bool isClimbing = false;

    Animation bodyAnim;
    Animator animator;
    public new Rigidbody2D rigidbody2D;
    CapsuleCollider2D capsuleCollider2D;

    float footstepDistTraveled = 0;

    int jumpsRemaining = 0;
    bool isJumping = false;
    bool jumpCanceled = false;
    float jumpTick = 0;

    bool hasMovedInAir = false;

    bool wantsJump = false;
    bool jumpPendingRelease = false;
    float xInputRaw = 0;
    float xInput = 0;
    float yInput = 0;

    bool isAttacking = false;
    bool pendingAttackDamage = false;
    float attackStartTime = 0;
    int attackComboIndex = -1;
    float attackInputTime = 0;

    float lastClimbTime = 0;
    float lastDamageTime = 0;

    Vector2 lastPosition;
    Vector2 desiredVelocity;
    Vector2 acceleration;
    Dictionary<string, float> speedBoosts = new Dictionary<string, float>();

    float stunnedUntilTime;

    public bool levelComplete;

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
        if (GameManager.Instance.isPaused)
            return;

        isAttacking = GetCurrentAttack() != null;

        if (levelComplete)
        {
            ClearInput();
            xInput = 1;
        }
        else
        {
#if DEBUG
            UpdateDebugInput();
#endif
            UpdateInput();

            UpdateSleep();

        }

        var isPlayerGrounded = grounder ? grounder.IsGrounded() : false;
        var isClimbGrounded = climbGrounder ? climbGrounder.IsGrounded() : false;

        UpdateMovement(isPlayerGrounded);
        UpdateClimbing(isPlayerGrounded, isClimbGrounded);
        UpdateJumping(isPlayerGrounded);
        UpdateAttacking();

        UpdateFacing();

        //update animator
        if (animator && animator.runtimeAnimatorController)
        {
            bool isMoving = Mathf.Abs(rigidbody2D.velocity.x / groundSpeed) > 0.1f;

            animator.SetBool("isGrounded", grounder.IsGrounded(0) && !isJumping);
            animator.SetFloat("moveSpeed", Mathf.Abs(rigidbody2D.velocity.x / groundSpeed));
            animator.SetFloat("velocityY", rigidbody2D.velocity.y);
            animator.SetBool("isIdle", grounder.IsGrounded() && !isMoving && !isAttacking);
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isSleeping", isSleeping);
            //animator.SetBool("isClimbing", isClimbing);
        }

        //Clamp to map bounds
        var mapBounds = GameManager.Instance.GetMapBounds();
        Vector2 clampedPos = rigidbody2D.position;
        if (rigidbody2D.position.x <= mapBounds.xMin) { clampedPos.x = mapBounds.xMin; desiredVelocity.x = Mathf.Max(0, desiredVelocity.x); }
        else if (rigidbody2D.position.x >= mapBounds.xMax) { clampedPos.x = mapBounds.xMax; desiredVelocity.x = Mathf.Min(0, desiredVelocity.x); }
        if (rigidbody2D.position.y <= mapBounds.yMin) { clampedPos.y = mapBounds.yMin; desiredVelocity.y = Mathf.Max(0, desiredVelocity.y); }
        else if (rigidbody2D.position.y >= mapBounds.yMax) { clampedPos.y = mapBounds.yMax; desiredVelocity.y = Mathf.Min(0, desiredVelocity.y); }

        //Update rigidbody
        rigidbody2D.isKinematic = isClimbing;
        lastPosition = rigidbody2D.position = clampedPos;
        desiredVelocity.y = Mathf.Max(desiredVelocity.y, -maxFallingSpeedY);
        rigidbody2D.velocity = desiredVelocity;
    }

    void UpdateInput()
    {
        //get input
        xInput = xInputRaw = Input.GetAxisRaw("Horizontal");
        if (xInputRaw != 0)
        {
            xInput = Mathf.Max(0.3f, Mathf.Abs(xInputRaw)) * Mathf.Sign(xInputRaw);
        }
        yInput = Input.GetAxis("Vertical");

        wantsJump = Input.GetButton("Jump");
        if (jumpPendingRelease && !wantsJump) jumpPendingRelease = false;
        //wantsJump = true; jumpPendingRelease = false;

        if (Input.GetButtonDown("Attack"))
        {
            attackInputTime = Time.time;
        }
    }

    void ClearInput()
    {
        xInputRaw = xInput = yInput = attackInputTime = 0;
        wantsJump = jumpPendingRelease = false;
    }

    void UpdateDebugInput()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            timeToSleep += 60;
        }
        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            speedBoosts.Add("DebugSprint", 3.0f);
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.Joystick1Button5))
        {
            speedBoosts.Remove("DebugSprint");
        }
    }

    public float TimeSinceLastStim()
    {
        return lastStimTime > 0 ? Time.time - lastStimTime : -1;
    }

    void UpdateSleep()
    {
        sleepTimer += Time.deltaTime;
        if(!isSleeping && sleepTimer > timeToSleep)
        {
            Sleep();
        }
        else if (isSleeping && sleepTimer > timeToSleep + 2.0f && !GetComponent<Health>().GetIsDead())
        {
            GetComponent<Health>().Kill(true);
        }
    }

    MeleeAttack GetCurrentAttack()
    {
        if (meleeAttacks.Length > 0 && attackComboIndex >= 0 && attackComboIndex < meleeAttacks.Length)
        {
            return meleeAttacks[attackComboIndex];
        }
        return null;
    }

    bool CanAttack()
    {
        var currentAttack = GetCurrentAttack();
        return !isSleeping && !pendingAttackDamage && !IsStunned() && (currentAttack == null || Time.time - attackStartTime > currentAttack.nextAttackDelay);
    }

    void UpdateAttacking()
    {
        var currentAttack = GetCurrentAttack();

        //Start attack
        bool wantsAttack = attackInputTime > 0 && Time.time - attackInputTime < 0.3f;
        if (CanAttack() && wantsAttack)
        {
            attackComboIndex++;
            if (attackComboIndex >= meleeAttacks.Length)
                attackComboIndex = 0;

            currentAttack = GetCurrentAttack();

            attackInputTime = 0;

            if(attackComboIndex == 0)
            {
                animator.SetTrigger("onAttack");
            }
            else
            {
                animator.SetTrigger("onAttackCombo");
            }

            attackStartTime = Time.time;
            pendingAttackDamage = true;

            if (xInputRaw != 0) isLookingRight = xInputRaw > 0;

            if (meleeAttackBox && currentAttack != null)
            {
                currentAttack.attackSound?.Play(transform.position);

                meleeAttackBox.DealDamage(new Damage(currentAttack.damage, gameObject, currentAttack.hitSound, currentAttack.knockback), currentAttack.damageDelay);

                if (grounder.IsGrounded() && !meleeAttackBox.IsTargetInRange(Vector2.one * -2.0f))
                {
                    desiredVelocity = new Vector2(isLookingRight ? Mathf.Max(desiredVelocity.x, currentAttack.playerSpeedX) : Mathf.Min(desiredVelocity.x, -currentAttack.playerSpeedX), desiredVelocity.y);
                }
            }
        }

        if (currentAttack != null)
        {
            float attackTime = Time.time - attackStartTime;

            //Reset attack
            if (attackTime > 0.6f) 
            {
                attackComboIndex = -1;
                pendingAttackDamage = false;
            }
            //Trigger damage
            else if (attackTime > 0.3f)
            {
                if (pendingAttackDamage)
                {
                    animator.ResetTrigger("onAttack");
                    animator.ResetTrigger("onAttackCombo");
                    pendingAttackDamage = false;
                }
            }
        }
    }

    void UpdateClimbing(bool isGrounded, bool isClimbingGrounded)
    {
        if (climber == null) return;

        //Update climbing
        bool wasClimbing = isClimbing;
        isClimbing |= yInput != 0 && (Time.time - lastClimbTime > climbExitTime);
        isClimbing &= climber.canClimb;

        //Update move exit case
        if (allowMoveToCancelClimb && xInputRaw != 0)
        {
            isClimbing = false;
        }
        //Update jump exit case
        else if (xInputRaw != 0 && wantsJump)
        {
            isClimbing = false;
        }
        //Update botom of ladder case
        else if (isGrounded && isClimbingGrounded && yInput < 0 && !climber.atTopOfLadder)
        {
            isClimbing = false;
        }
        //Update top of ladder case
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

    void UpdateJumping(bool isGrounded)
    {
        //Reset jump
        if (!isJumping && jumpsRemaining != maxJumps && (isGrounded || isClimbing))
        {
            print("Jump Reset");
            jumpsRemaining = maxJumps;
        }

        //Start jump
        if (CanJump() && wantsJump && !jumpPendingRelease && jumpsRemaining > 0)
        {
            print(string.Format("Jump Started ({0}/{1})", maxJumps - jumpsRemaining, maxJumps));
            jumpTick = 0;
            isJumping = true;
            jumpsRemaining--;
            jumpPendingRelease = true; //latch the input

            //Zero velocity if we're opposing it - this allows the player to make move accurate jumps
            if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(desiredVelocity.x))
            {
                desiredVelocity.x = -desiredVelocity.x;
            }

            OnJump();
        }
        
        if (isJumping)
        {
            float lastJumpTick = jumpTick;
            jumpTick = Mathf.Clamp(jumpTick + Time.deltaTime, 0, maxJumpTime);

            //Cancel jump
            if (jumpCanceled || !wantsJump && jumpTick >= minJumpTime)
            {
                jumpCanceled = true;
                if (desiredVelocity.y > 0.01f)
                {
                    desiredVelocity.y -= gravity * 3.0f * Time.deltaTime;
                }
                else
                {
                    print("Jump Canceled");
                    isJumping = false;
                    jumpCanceled = false;
                }
                return;
            }
            //End jump
            else if (jumpTick >= maxJumpTime)
            {
                print("Jump Ended");
                isJumping = false;
                desiredVelocity.y = 0; //enforce jump height
            }
            //Do jump
            else
            {
                float deltaY = (jumpCurve.Evaluate(jumpTick / maxJumpTime) - jumpCurve.Evaluate(lastJumpTick / maxJumpTime)) * jumpHeight;
                desiredVelocity.y = deltaY / Time.deltaTime;
            }
        }
    }

    bool CanMove()
    {
        return !isSleeping && !isClimbing && !isAttacking && !IsStunned();
    }

    void UpdateMovement(bool isGrounded)
    {
        bool airAttackHang = false;
        var currentAttack = GetCurrentAttack();
        if (currentAttack != null && meleeAttackBox)
        {
            airAttackHang = meleeAttackBox.lastHitTime > 0 && Time.time - meleeAttackBox.lastHitTime < Mathf.Max(currentAttack.nextAttackDelay - 0.2f, 0.1f);
        }

        //apply gravity
        if (!isGrounded)
        {
            desiredVelocity.y -= gravity * Time.deltaTime * (airAttackHang ? 0.1f : 1.0f);
        }

        capsuleCollider2D.sharedMaterial = isGrounded ? groundedPhysicsMaterial : airPhysicsMaterial;

        //Update movement
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

            //use air speed when changing direction in the air
            if (!isGrounded && (hasMovedInAir || Mathf.Sign(desiredVelocity.x) != Mathf.Sign(xInput)))
            {
                desiredSpeed = airSpeed;
                hasMovedInAir = true;
            }

            foreach(var boost in speedBoosts.Values)
            {
                desiredSpeed *= boost;
            }

            Vector2 desiredInputVelocity = new Vector2(xInput * desiredSpeed, isGrounded && !isJumping ? 0 : desiredVelocity.y);

            var groundHit = grounder.GetGroundHit();
            float angleUp = groundHit ? Vector2.Angle(groundHit.normal, Vector2.up) : 0;
            if (groundHit && angleUp < maxFloorSlopeAngle && xInput != 0 && !isJumping)
            {
                Vector2 tangent = Vector2.Perpendicular(groundHit.normal);

                desiredVelocity = -tangent.normalized * xInput * desiredSpeed;

                Debug.DrawLine(transform.position, (Vector2)transform.position - tangent * 2.0f * Mathf.Sign(xInput), Color.magenta);

                //desiredVelocity = Vector2.SmoothDamp(desiredVelocity, desiredInputVelocity, ref acceleration, 0.1f, float.MaxValue, Time.deltaTime);

                float py = groundHit.point.y - grounder.transform.localPosition.y;
                transform.position = new Vector3(transform.position.x, py, transform.position.z);
            }
            else if (xInput != 0)
            {
                desiredVelocity = Vector2.SmoothDamp(desiredVelocity, desiredInputVelocity, ref acceleration, isGrounded ? 0.2f : 0.2f, float.MaxValue, Time.deltaTime);
            }
            else
            {
                desiredVelocity = Vector2.SmoothDamp(desiredVelocity, desiredInputVelocity, ref acceleration, isGrounded ? 0.05f : 0.5f, float.MaxValue, Time.deltaTime);
            }
        }
        else
        {
            xInput = 0;
            if (isGrounded || isAttacking)
            {
                desiredVelocity = Vector2.SmoothDamp(desiredVelocity, new Vector2(0, desiredVelocity.y), ref acceleration, 0.2f, float.MaxValue, Time.deltaTime);
            }
        }

        //Update footstep sounds
        bool isMoving = Mathf.Abs(rigidbody2D.velocity.x) > 0.0f || (xInput != 0 && Mathf.Sign(rigidbody2D.velocity.x) != Mathf.Sign(xInput));
        if (isGrounded && isMoving)
        {
            float deltaX = (rigidbody2D.position - lastPosition).magnitude;
            footstepDistTraveled += deltaX;
            if (footstepDistTraveled > footstepInterval)
            {
                footstepDistTraveled -= footstepInterval;
                footStepAudio.Play(transform.position);
            }
        }
        else
        {
            footstepDistTraveled = 0;
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
        if (!isJumping)
        {
            landAudio?.Play(transform.position, Mathf.Max(0.3f, Mathf.Abs(desiredVelocity.y / maxFallingSpeedY)));
            footStepAudio.Play(transform.position);
            desiredVelocity.y = 0;
            rigidbody2D.velocity = desiredVelocity;
        }
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
            desiredVelocity.y = Mathf.Sign(knockback.y) * knockbackVector.y;
            rigidbody2D.velocity = desiredVelocity;
            StunFor(stunDuration);
        }
        sleepTimer += 1.0f;
        damage.value = 0; //skip applying health damage

        //animator.SetTrigger("onDamage");
    }

    void OnDeath()
    {
        //this.enabled = false;
        //FindObjectOfType<GameManager>().OnLoss("You Died.");
    }

    void OnAttackHit(Damage damage)
    {
        if(!grounder.IsGrounded())
        {
            desiredVelocity.y = gravity * 0.02f;
            desiredVelocity.x = 0;
        }
        desiredVelocity.x *= 0.5f;
    }

    void OnDeathAnimFinished()
    {
        //Destroy(gameObject);
    }

    void OnFallingAsleepExit()
    {
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
        sleepTimer = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach(var contact in collision.contacts)
        {
            float angle = Vector2.Angle(contact.normal, Vector2.up);

            if (angle > 180 - (90 - maxFloorSlopeAngle)) //touched ceiling
            {
                //isJumping = false;
                desiredVelocity.y = 0;
                Debug.DrawLine(contact.point, contact.point + contact.normal * 1.0f, Color.yellow, 3.0f);
            }
        }
    }
}
