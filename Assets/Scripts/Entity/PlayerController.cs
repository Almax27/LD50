using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public SpriteRenderer body;
    public SpriteRenderer legs;
    public Grounder grounder;
    public Climber climber;
    public Grounder climbGrounder;

    [Header("Config")]
    public float gravity = 1;
    public AnimationCurve jumpCurve = new AnimationCurve();
    public float jumpHeight = 1;
    public float groundSpeed = 0.3f;
    public float airSpeed = 0.1f;
    public float climbingSpeed = 0.2f;
    public bool allowMoveToCancelClimb = false;
    public float climbExitTime = 0.2f; //time after leaving climbable before can climb again
    public float rootOnDamageTime = 0.2f;
    public GameObject[] spawnOnJump = new GameObject[0];

    [Header("State")]
    public bool canMove = true;
    public bool canLook = true;
    public bool canJump = true;
    public bool isMovingRight = true;
    public bool isLookingRight = true;
    public bool isClimbing = false;

    Animator animator;
    new Rigidbody2D rigidbody2D;

    int jumpsRemaining = 0;
    int maxJumps = 1;
    bool hasJumped = false;
    float jumpTick = 0;

    bool hasMovedInAir = false;

    bool tryJump = false;
    float xInputRaw = 0;
    float xInput = 0;
    float xInputVel = 0;
    float yInput = 0;

    float lastClimbTime = 0;
    float lastDamageTime = 0;

    Vector2 velocity = Vector2.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        //get input
        xInputRaw = Input.GetAxisRaw("Horizontal");
        xInput = Mathf.SmoothDamp(xInput, xInputRaw, ref xInputVel, 0.1f, float.MaxValue, Time.deltaTime);
        yInput = Input.GetAxis("Vertical");
        tryJump = tryJump || Input.GetButtonDown("Jump");

        //update animator
        if (animator && animator.runtimeAnimatorController)
        {
            animator.SetFloat("moveSpeed", Mathf.Abs(xInput));
            animator.SetFloat("climbSpeed", Mathf.Abs(yInput));
        }
    }

    void FixedUpdate()
    {
        var isGrounded = grounder ? grounder.isGrounded && velocity.y <= 0 : false;
        var isClimbGrounded = climbGrounder ? climbGrounder.isGrounded && velocity.y <= 0 : false;

        //apply gravity
        if (isGrounded)
        {
            velocity.y = 0;
        }
        if (!isClimbing)
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        HandleClimbing(isGrounded, isClimbGrounded);
        HandleJumping(isGrounded);
        HandleMovement(isGrounded);

        UpdateFacing();

        //update animator
        if (animator && animator.runtimeAnimatorController)
        {
            animator.SetBool("isRunning", canMove && xInputRaw != 0);
            animator.SetBool("isGrounded", isGrounded);
            animator.SetBool("isClimbing", isClimbing);
        }

        //move rigidbody
        rigidbody2D.isKinematic = isClimbing;
        Vector3 newPosition = transform.position + new Vector3(velocity.x, velocity.y, 0) * Time.fixedDeltaTime;
        rigidbody2D.MovePosition(newPosition);
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
        else if (xInputRaw != 0 && tryJump)
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
            velocity.y = yInput * climbingSpeed;

            //snap to ladder
            var pos = transform.position;
            pos.x = climber.lockX;
            transform.position = pos;

            canMove = false;
            tryJump = false;
            lastClimbTime = Time.time;
        }
        if (wasClimbing != isClimbing)
        {
            velocity.y = 0;
        }
    }

    void HandleJumping(bool isGrounded)
    {
        if (isGrounded || isClimbing)
        {
            jumpsRemaining = maxJumps;
            hasJumped = false;
        }
        if (canJump && tryJump && jumpsRemaining > 0)
        {
            jumpTick = 0;
            hasJumped = true;
            jumpsRemaining--;
            OnJump();
        }
        if (hasJumped)
        {
            float lastJumpTick = jumpTick;
            jumpTick += Time.fixedDeltaTime;
            float deltaY = (jumpCurve.Evaluate(jumpTick) - jumpCurve.Evaluate(lastJumpTick)) * jumpHeight;
            if (deltaY != 0)
            {
                velocity.y = deltaY / Time.fixedDeltaTime;
            }
        }
        tryJump = false;
    }

    void HandleMovement(bool isGrounded)
    {
        bool isRooted = Time.time - lastDamageTime < rootOnDamageTime;

        //handle movement
        if (isGrounded)
        {
            hasMovedInAir = false;
        }
        if (canMove && !isRooted)
        {
            float desiredSpeed = groundSpeed;
            //use air speed when changing direction in the air
            if (!isGrounded && (hasMovedInAir || Mathf.Sign(velocity.x) != Mathf.Sign(xInput)))
            {
                desiredSpeed = airSpeed;
                hasMovedInAir = true;
            }
            velocity.x = xInput * desiredSpeed;
        }
        else
        {
            xInput = 0;
            velocity.x = 0;
        }
    }

    void UpdateFacing()
    {
        if (Mathf.Abs(xInput) > 0.0001f)
        {
            isMovingRight = xInput > 0;
        }
        if (canLook && Mathf.Abs(xInputRaw) > 0.0001f)
        {
            isLookingRight = xInputRaw > 0;
        }
        if(body) body.flipX = !isLookingRight;
        if(legs) legs.flipX = !isMovingRight;
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
    }

    public virtual void OnDamage(Damage damage)
    {
        lastDamageTime = Time.time;
        //animator.SetTrigger("onDamage");
    }

    void OnDeath()
    {
        if (animator && animator.runtimeAnimatorController)
        {
            animator.SetTrigger("onDeath");
        }

        this.enabled = false;
        FindObjectOfType<GameManager>().OnLoss("You Died.");
    }

    void OnDeathAnimFinished()
    {
        //Destroy(gameObject);
    }
}
