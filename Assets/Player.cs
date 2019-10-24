using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject graphics;
    public GameObject jumpParticle;

    public float moveAcceleration;
    public float maxMoveSpeed;
    public float jumpForce;
    public float fallMultiplier;
    public float lowJumpMultiplier;

    public float animSpeed;
    public Vector2 stretchScale;
    public Vector2 squashScale;

    private Rigidbody2D rb;
    private Vector2 graphicsDefaultScale;
    private float gravityDefaultScale;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityDefaultScale = rb.gravityScale;
        graphicsDefaultScale = graphics.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        HandleControls();

        if(Input.GetKey(KeyCode.Space))
        {
            graphics.transform.localScale = Vector2.Lerp(graphics.transform.localScale, stretchScale, animSpeed);
        } else
        {
            graphics.transform.localScale = Vector2.Lerp(graphics.transform.localScale, graphicsDefaultScale, animSpeed);
        }
    }

    private void FixedUpdate()
    {
        HandleJumpArc();
        LimitMoveSpeed();
    }

    void HandleControls()
    {
        if (Input.GetKey("a"))
        {
            rb.AddForce(-Vector2.right * moveAcceleration);
        }

        if (Input.GetKey("d"))
        {
            rb.AddForce(Vector2.right * moveAcceleration);
        }

        if (Input.GetKeyDown("space"))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if(Input.GetKeyDown("space"))
        {
            Instantiate(jumpParticle, transform.position, jumpParticle.transform.rotation);
        }
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

    void LimitMoveSpeed()
    {
        float xVelocity = Mathf.Min(Mathf.Abs(rb.velocity.x), maxMoveSpeed) * Mathf.Sign(rb.velocity.x);
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }


}
