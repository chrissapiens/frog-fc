using UnityEngine;

// Keeps the ball sane: caps its top speed, remembers where it starts,
// and recovers if it escapes the arena or gets stuck sitting still.
public class Ball : MonoBehaviour
{
    [Header("Speed Limit")]
    [Tooltip("The ball uses Discrete collision so fast shots can skip over the " +
             "holes. That same setting lets a very fast ball punch through a wall, " +
             "so cap the speed below the point where that happens.")]
    public float maxSpeed = 18f;

    [Header("Safety Net")]
    public float killY = -12f;            // fell out of the world
    public float stuckSpeed = 0.4f;       // "basically not moving"
    public float stuckSeconds = 5f;       // how long before we intervene

    Rigidbody2D rb;
    Vector2 startPos;
    float stuckTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        // Clamp in FixedUpdate so it runs on the same clock as the physics engine
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void Update()
    {
        if (transform.position.y < killY)
        {
            ResetToCenter();
            return;
        }

        // Anti-stalemate: if nobody has moved the ball in a while, re-drop it
        if (rb.linearVelocity.magnitude < stuckSpeed)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckSeconds) ResetToCenter();
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    public void ResetToCenter()
    {
        stuckTimer = 0f;
        rb.position = startPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
