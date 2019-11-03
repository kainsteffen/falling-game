using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public StateMachine stateMachine = new StateMachine();

    private State currentState;

    public GameObject graphics;
    public GameObject eyesLookingLeft;
    public GameObject eyesLookRight;
    public ParticleSystem jumpParticle;

    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;

    public bool isWallSliding;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckRadius;
    public LayerMask wallLayer;

    public Vector2 wallJumpDirection;
    public float walljumpForce;

    public float moveAcceleration;
    public float maxMoveSpeed;

    public float jumpForce;
    public float jumpDefaultCount;
    private float jumpCounter;


    public float fallMultiplier;
    public float lowJumpMultiplier;

    
    public Vector2 crouchScale;
    private Vector2 defaultScale;

    public float animSpeed;
    public Vector2 stretchScale;
    public Vector2 squashScale;

    private Vector2 lookDirection;

    private Rigidbody2D rb;
    private Vector2 graphicsDefaultScale;
    private float gravityDefaultScale;
    private float defaultDrag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        gravityDefaultScale = rb.gravityScale;
        defaultScale = transform.localScale;
        defaultDrag = rb.drag;
        graphicsDefaultScale = graphics.transform.localScale;

        stateMachine.ChangeState(new GroundedState(this));
    }


    void Update()
    {
        HandleControls();
    }

    private void FixedUpdate()
    {
        stateMachine.Update();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheckLeft.position, wallCheckRadius);
        Gizmos.DrawWireSphere(wallCheckRight.position, wallCheckRadius);
    }

    // TODO: fix jumpCounter resetting before leaving ground
    public bool CheckGround()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    public Vector2 CheckWall()
    {
        if(Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer))
        {
            return -Vector2.right;
        }

        if(Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer))
        {
            return Vector2.right;
        }

        return Vector2.zero;
    }

    public void HandleMovementInput()
    {
        if (Input.GetKey("a"))
        {
            Move(-Vector2.right);
        }

        if (Input.GetKey("d"))
        {
            Move(Vector2.right);
        }
    }

    public void HandleJumpInput()
    {
        if (Input.GetKeyDown("space"))
        {
            Jump();
        }
    }

    public void HandleWallJumpInput()
    {
        if (Input.GetKeyDown("space"))
        {
            WallJump();
        }
    }

    void CrouchJump()
    {
        if (Input.GetKeyDown("s"))
        {
            Crouch();
        }
        else
        {
            transform.localScale = Vector2.Lerp(transform.localScale, defaultScale, animSpeed);
        }
    }

    void HandleControls()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            graphics.transform.localScale = Vector2.Lerp(graphics.transform.localScale, stretchScale, animSpeed);
        }
        else
        {
            graphics.transform.localScale = Vector2.Lerp(graphics.transform.localScale, graphicsDefaultScale, animSpeed);
        }
    }

    void Move(Vector2 direction)
    {
        SetLookDirection(direction);
        rb.AddForce(lookDirection * moveAcceleration);
        LimitMoveSpeed();
    }

    void Jump()
    {
        if (jumpCounter > 0)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpParticle.Play();
            jumpCounter--;
        }
    }

    public void ResetJumpCounter()
    {
        jumpCounter = jumpDefaultCount;
    }

    void WallJump()
    {
        rb.AddForce(new Vector2(wallJumpDirection.x * lookDirection.x, wallJumpDirection.y) * walljumpForce, ForceMode2D.Impulse);
        jumpParticle.Play();
    }

    void Crouch()
    {
        transform.localScale = Vector2.Lerp(transform.localScale, crouchScale, animSpeed);
    }

    void HandleJumpArc()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = gravityDefaultScale;
        }
    }

    public void SetLookDirection(Vector2 direction)
    {
        lookDirection = direction;
        if(lookDirection == Vector2.right)
        {
            eyesLookingLeft.SetActive(false);
            eyesLookRight.SetActive(true);
        } else if (lookDirection == -Vector2.right)
        {
            eyesLookingLeft.SetActive(true);
            eyesLookRight.SetActive(false);
        }
    }

    public void StartWallSlide()
    {
        rb.velocity = new Vector2(0, -0.5f);
        rb.gravityScale = 0;
        rb.drag = 0;
    }

    public void StopWallSlide()
    {
        rb.gravityScale = gravityDefaultScale;
        rb.drag = defaultDrag;
    }

    void LimitMoveSpeed()
    {
        float xVelocity = Mathf.Min(Mathf.Abs(rb.velocity.x), maxMoveSpeed) * Mathf.Sign(rb.velocity.x);
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }
}

public class GroundedState : State
{
    Player owner;

    public GroundedState(Player owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.ResetJumpCounter();
    }

    public void Execute()
    {
        owner.HandleMovementInput();
        owner.HandleJumpInput();

        if (!owner.CheckGround())
        {
            owner.stateMachine.ChangeState(new JumpingState(owner));
        }
    }

    public void Exit()
    {

    }
}

public class JumpingState : State
{
    Player owner;

    public  JumpingState(Player owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        owner.HandleMovementInput();
        owner.HandleJumpInput();

        if (owner.CheckGround())
        {
            owner.stateMachine.ChangeState(new GroundedState(owner));
        }

        Vector2 detectedWall = owner.CheckWall();
        if (detectedWall != Vector2.zero)
        {
            owner.SetLookDirection(-detectedWall);
            owner.stateMachine.ChangeState(new WallSlidingState(owner));
        }
    }

    public void Exit()
    {

    }
}

public class WallSlidingState : State{
    Player owner;

    public WallSlidingState(Player owner)
    {
        this.owner = owner;
    }
    
    public void Enter()
    {
        owner.StartWallSlide();
    }

    public void Execute()
    {
        owner.HandleWallJumpInput();

        if (owner.CheckWall() == Vector2.zero)
        {
            if (owner.CheckGround())
            {
                owner.stateMachine.ChangeState(new GroundedState(owner));
            } else
            {
                owner.stateMachine.ChangeState(new JumpingState(owner));
            }
        }
    }

    public void Exit()
    {
        owner.StopWallSlide();
    }
}
