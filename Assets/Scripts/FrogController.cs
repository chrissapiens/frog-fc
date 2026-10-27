using UnityEngine;
using UnityEngine.InputSystem;

// handles one frog's movement, double jump, and headbutt.
// the tongue lives in its own script (Tongue.cs) on the same object.
public class FrogController : MonoBehaviour
{
    // keys
    public Key leftKey  = Key.A;
    public Key rightKey = Key.D;
    public Key jumpKey  = Key.W;

   // movement
    public float moveSpeed       = 7f;
    public float firstJumpForce  = 13f;
    public float secondJumpForce = 9f;    // weaker second jump

    public float airAcceleration = 20f;

    public float headbuttForce = 15f;     // strong but imprecise headbutt

    public float headbuttCooldown = 0.25f; // headbutt cooldown to prevent ball from relaunching 
                                          // into the wall when a player is pressed up on it

    public float knockbackTime = 0.3f; // time a player can't move horizontally after being tongue hit

    public Transform groundCheck;         // empty child at the frog's feet
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    // Tongue.cs reads this to know which way to fire when no direction is held
    public int Facing { get; private set; } = 1;

    Rigidbody2D rb;
    int jumpsUsed;
    float moveInput;
    float headbuttTimer;
    float knockbackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        headbuttTimer -= Time.deltaTime;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // read horizontal input
        moveInput = 0f;
        if (kb[leftKey].isPressed)  moveInput -= 1f;
        if (kb[rightKey].isPressed) moveInput += 1f;

        if (moveInput != 0f) Facing = (int)Mathf.Sign(moveInput);

        // ground check resets the double jump
        if (IsGrounded() && rb.linearVelocity.y <= 0.01f) jumpsUsed = 0;

        if (kb[jumpKey].wasPressedThisFrame && jumpsUsed < 2)
        {
            float force = (jumpsUsed == 0) ? firstJumpForce : secondJumpForce;

            // clear downward speed (double jump always feels the same)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            jumpsUsed++;
        }
    }

    // shoving a frog with tognue, hands physics over to Unity
    public void ApplyKnockback(Vector2 impulse)
    {
        knockbackTimer = knockbackTime;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

// checks if the ground is under teh frog's feet  (frog layer counts too)
// you can land another player's head, skips my own collider
    bool IsGrounded()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            groundCheck.position, groundCheckRadius, groundLayer);

        foreach (Collider2D c in hits)
        {
            if (c == null) continue;
            if (c.transform.IsChildOf(transform)) continue;   // that's me
            return true;
        }

        return false;
    }

    void FixedUpdate()
    {
        // when being knocked back, physics moves the frog
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // ground = instant control 
        // air = slow steering, only while holding a key
        bool airborne = jumpsUsed > 0;

        if (!airborne)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (moveInput != 0f)
        {
            float steered = Mathf.MoveTowards(
                rb.linearVelocity.x,
                moveInput * moveSpeed,
                airAcceleration * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector2(steered, rb.linearVelocity.y);
        }
    }

    // any frog contact with the ball launches it away from it (headbutt)
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!coll.gameObject.CompareTag("Ball")) return;
        if (headbuttTimer > 0f) return;

        Rigidbody2D ballRb = coll.rigidbody;
        if (ballRb == null) return;

        headbuttTimer = headbuttCooldown;

        Vector2 away = (ballRb.position - rb.position).normalized;

      // set the velocity instead of adding to it; every headbutt hits the same
        ballRb.linearVelocity = away * headbuttForce;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
