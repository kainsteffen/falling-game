using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public InputMaster controls = null;
    public StateMachine stateMachine = new StateMachine();

    public TouchController touchController;
    public TimeController timeManager;

    public GameObject projectile;
    public GameObject graphics;
    public GameObject eyesLookingLeft;
    public GameObject eyesLookRight;
    public ParticleSystem jumpParticle;

    public float killRadius;
    public LayerMask killLayer;

    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public LayerMask absoluteGroundLayer;

    public bool isWallSliding;
    public float wallSlideGravity;
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

    public float shootForce;

    public float slideForce;
    public float slideDuration;
    public float slideTimer;
    public Vector2 slideScale;
    private Vector2 defaultScale;

    public float animSpeed;
    public Vector2 stretchScale;
    public Vector2 squashScale;

    private Vector2 lookDirection;

    private Rigidbody2D rb;
    private LineRenderer lr;
    private Vector2 graphicsDefaultScale;
    private float gravityDefaultScale;
    private float defaultDrag;

    public float aimLineLength;
    private Vector2 aimDirection;
    private Vector2 dragStartPosition;
    private bool focusMode;

    void Awake()
    {
        controls = new InputMaster();
        rb = GetComponent<Rigidbody2D>();
        lr = GetComponent<LineRenderer>();

        gravityDefaultScale = rb.gravityScale;
        defaultScale = transform.localScale;
        defaultDrag = rb.drag;
        graphicsDefaultScale = graphics.transform.localScale;

        slideTimer = slideDuration;

        stateMachine.ChangeState(new GroundedState(this));
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        stateMachine.Update();
        HandleSlowMoInput();
       // HandleShootInput();
       if(focusMode)
        {
            if (Input.GetMouseButton(0))
            {
                aimDirection = dragStartPosition - (Vector2) Input.mousePosition;
                DrawLine(transform.position, ((Vector2)transform.position + (aimDirection.normalized * aimLineLength)));
            }
            if(Input.GetMouseButtonUp(0))
            {
                focusMode = false;
                lr.enabled = false;
                Shoot(aimDirection.normalized);
            }
        }
        DetectEnemies();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheckLeft.position, wallCheckRadius);
        Gizmos.DrawWireSphere(wallCheckRight.position, wallCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }

    void DetectEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, killRadius, killLayer);
        foreach(Collider2D collider in colliders)
        {
            collider.GetComponent<Enemy>().TakeDamage(1);
        }
    }

    IEnumerator JumpOff()
    {
        print(gameObject.layer);
        print(groundLayer.value);
        Physics2D.IgnoreLayerCollision(gameObject.layer, 8, true);
        yield return new WaitForSeconds(.5f);
        Physics2D.IgnoreLayerCollision(gameObject.layer, 8, false);
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    // TODO: fix jumpCounter resetting before leaving ground
    public bool CheckGround()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null 
            || Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, absoluteGroundLayer);
    }

    public Vector2 CheckWall()
    {
        if (Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer))
        {
            SetLookDirection(Vector2.right);
            return -Vector2.right;
        }

        if (Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer))
        {
            SetLookDirection(-Vector2.right);
            return Vector2.right;
        }

        return Vector2.zero;
    }

    public void HandleSlowMoInput()
    {
        if(touchController.GetLongPress())
        {
            focusMode = true;
            lr.enabled = true;
            dragStartPosition = Input.mousePosition;
            timeManager.StartSlowMotion();
        }
    }

    public void HandleJumpOffInput()
    {
        if(touchController.GetSwipeDown())
        {
            StartCoroutine(JumpOff());
        }
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
        if (touchController.GetTap())
        {
            Jump();
        }
    }

    public void JumpAnimation()
    {
        if (Input.GetMouseButton(0))
        {
            graphics.transform.DOScale(stretchScale, 0.1f).SetEase(Ease.InOutQuad);
        }
        else
        {
            graphics.transform.DOScale(graphicsDefaultScale, 0.1f);
        }
    }

    public void HandleWallJumpInput()
    {
        if (touchController.GetTap())
        {
            WallJump();

        }
    }

    //void HandleShootInput()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 screenPoint = Input.mousePosition;
    //        screenPoint.z = Mathf.Abs(Camera.main.transform.position.z); //distance of the plane from the camera
    //        Vector3 direction = Vector3.Normalize(Camera.main.ScreenToWorldPoint(screenPoint) - transform.position);
    //        Shoot(direction);
    //    }
    //}

    public void HandSlideInput()
    {
        
        if(touchController.GetSwipeRight())
        {
            rb.AddForce(Vector2.right * slideForce, ForceMode2D.Impulse);
            stateMachine.ChangeState(new SlidingState(this));
        }
    }

    void Shoot(Vector2 direction)
    {
        GameObject newProjectile = Instantiate(projectile, transform.position, transform.rotation);
        newProjectile.GetComponent<Rigidbody2D>().AddForce(direction * shootForce);
        newProjectile.tag = "Player";
    }

    public void Move(Vector2 direction)
    {
        SetLookDirection(direction);
        rb.AddForce(lookDirection * moveAcceleration);
        LimitMoveSpeed();
    }

    void Jump()
    {
        if (jumpCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpParticle.Play();
            jumpCounter--;
        }
    }

    public void ScaleToDefault()
    {
        DOTween.To(() => transform.localScale, x => transform.localScale = x, (Vector3)defaultScale, .1f);
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

    //void HandleJumpArc()
    //{
    //    if (rb.velocity.y < 0)
    //    {
    //        rb.gravityScale = fallMultiplier;
    //    }
    //    else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
    //    {
    //        rb.gravityScale = lowJumpMultiplier;
    //    }
    //    else
    //    {
    //        rb.gravityScale = gravityDefaultScale;
    //    }
    //}

    public void SetLookDirection(Vector2 direction)
    {
        lookDirection = direction;
        if (lookDirection == Vector2.right)
        {
            eyesLookingLeft.SetActive(false);
            eyesLookRight.SetActive(true);
        }
        else if (lookDirection == -Vector2.right)
        {
            eyesLookingLeft.SetActive(true);
            eyesLookRight.SetActive(false);
        }
    }

    public void StartWallSlide()
    {
        if (CheckGround())
        {
            rb.velocity = new Vector2(0, rb.velocity.x / 2);
        }
        else
        {
            print("zero");
            rb.velocity = Vector2.zero;
        }

        rb.gravityScale = wallSlideGravity;
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
        owner.ScaleToDefault();
        owner.Move(Vector2.right);
        owner.HandleJumpInput();
        owner.HandSlideInput();
        owner.HandleJumpOffInput();
        owner.JumpAnimation();

        if (!owner.CheckGround())
        {
            owner.stateMachine.ChangeState(new JumpingState(owner));
        }

        Vector2 detectedWall = owner.CheckWall();
        if (owner.CheckWall() != Vector2.zero)
        {
            owner.SetLookDirection(-detectedWall);
            owner.stateMachine.ChangeState(new WallSlidingState(owner));
        }
    }

    public void Exit()
    {

    }
}

public class JumpingState : State
{
    Player owner;

    public JumpingState(Player owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        owner.Move(Vector2.right);
        owner.HandleJumpInput();
        owner.JumpAnimation();

        if (owner.CheckGround())
        {
            owner.stateMachine.ChangeState(new GroundedState(owner));
        }

        if (owner.CheckWall() != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new WallSlidingState(owner));
        }
    }

    public void Exit()
    {

    }
}

public class WallSlidingState : State
{
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
        owner.ScaleToDefault();
        owner.HandleWallJumpInput();

        if (owner.CheckWall() == Vector2.zero)
        {
            if (owner.CheckGround())
            {
                owner.stateMachine.ChangeState(new GroundedState(owner));
            }
            else
            {
                owner.stateMachine.ChangeState(new WallJumpingState(owner));
            }
        }
    }

    public void Exit()
    {
        owner.StopWallSlide();
    }
}

public class WallJumpingState : State
{
    Player owner;

    public WallJumpingState(Player owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        owner.ScaleToDefault();
        if (owner.CheckGround())
        {
            owner.stateMachine.ChangeState(new GroundedState(owner));
        }

        if (owner.CheckWall() != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new WallSlidingState(owner));
        }
    }

    public void Exit()
    {

    }
}

public class SlidingState : State
{
    Player owner;
    float timer;

    public SlidingState(Player owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        timer = owner.slideDuration;
    }

    public void Execute()
    {
        DOTween.To(() => owner.graphics.transform.localScale, x => owner.graphics.transform.localScale = x, (Vector3)owner.slideScale, .1f);
        timer -= Time.deltaTime;
        owner.Move(Vector2.right);
        if (timer < 0)
        {            
            if(owner.CheckGround())
            {
                owner.stateMachine.ChangeState(new GroundedState(owner));
            }
        }

        if (!owner.CheckGround() && owner.CheckWall() == Vector2.zero)
        {
            owner.stateMachine.ChangeState(new JumpingState(owner));
        }
        else if (owner.CheckWall() != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new WallSlidingState(owner));
        }
    }

    public void Exit()
    {

    }
}