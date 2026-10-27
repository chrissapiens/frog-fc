using UnityEngine;
using UnityEngine.InputSystem;

// Handles one frog's movement, double jump, and headbutt.
// The tongue lives in its own script (Tongue.cs) on the same object.
public class FrogController : MonoBehaviour
{
    [Header("Keys")]
    public Key leftKey  = Key.A;
    public Key rightKey = Key.D;
    public Key jumpKey  = Key.W;

    [Header("Movement")]
    public float moveSpeed       = 7f;
    public float firstJumpForce  = 13f;
    public float secondJumpForce = 9f;    // weaker, so it reads as a hop

    [Tooltip("How fast the frog can change direction while airborne, in units " +
             "per second squared. Lower feels more committed to the jump arc; " +
             "higher feels floaty and arcade-y. Set to 0 for no air control.")]
    public float airAcceleration = 25f;

    [Header("Headbutt")]
    public float headbuttForce = 15f;     // strong but imprecise

    [Tooltip("Minimum gap between headbutts. Without this, a frog pressed " +
             "against a wall re-launches the ball into that wall every frame, " +
             "and the physics solver lets them overlap.")]
    public float headbuttCooldown = 0.25f;

    [Header("Knockback")]
    [Tooltip("How long after being tongued this frog stops driving its own " +
             "horizontal speed. Without this window, FixedUpdate overwrites the " +
             "knockback velocity on the very next physics step and nothing moves.")]
    public float knockbackTime = 0.3f;

    [Header("Ground Check")]
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

        // Read horizontal input
        moveInput = 0f;
        if (kb[leftKey].isPressed)  moveInput -= 1f;
        if (kb[rightKey].isPressed) moveInput += 1f;

        if (moveInput != 0f) Facing = (int)Mathf.Sign(moveInput);

        // Ground check resets the double jump
        if (IsGrounded() && rb.linearVelocity.y <= 0.01f) jumpsUsed = 0;

        if (kb[jumpKey].wasPressedThisFrame && jumpsUsed < 2)
        {
            float force = (jumpsUsed == 0) ? firstJumpForce : secondJumpForce;

            // Clear downward speed first so a double jump always feels the same
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            jumpsUsed++;
        }
    }

    // Called by another frog's Tongue. Shoves this frog and briefly hands
    // control of its horizontal speed over to the physics engine.
    public void ApplyKnockback(Vector2 impulse)
    {
        knockbackTimer = knockbackTime;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    // True when something in the Ground Layer mask is under the frog's feet.
    // The Frog layer is in that mask so you can land on the other player's head,
    // which means this has to skip the frog's own collider - the ground check
    // sits at the bottom edge and overlaps it, which would otherwise read as
    // permanently grounded and give infinite jumps.
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
        // While being knocked back, let physics move the frog instead of
        // forcing its velocity from input
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // On the ground, drive horizontal speed straight from input.
        // In the air, only steer toward the target speed, and only while a
        // direction is actually held, so letting go preserves the jump arc.
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

    // The headbutt. Any body contact with the ball launches it away from the frog.
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!coll.gameObject.CompareTag("Ball")) return;
        if (headbuttTimer > 0f) return;

        Rigidbody2D ballRb = coll.rigidbody;
        if (ballRb == null) return;

        headbuttTimer = headbuttCooldown;

        Vector2 away = (ballRb.position - rb.position).normalized;

        // Replace the ball's velocity instead of adding to it, so a headbutt
        // always sends it the same distance regardless of how it arrived.
        ballRb.linearVelocity = away * headbuttForce;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
