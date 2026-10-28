using UnityEngine;

// - caps the ball's top speed
// - the ball recovers if it escapes, gets stuck, or is sitting still
public class Ball : MonoBehaviour
{
    public float maxSpeed = 18f;

    public float killY = -12f;            // fell out of the world
    public float stuckSpeed = 0.1f;       // ball is stuck
    public float stuckSeconds = 15f;      // how long before we intervene

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
        // FixedUpdate runs on the physics clock; the cap applies to every step
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

        // if nobody moves the ball in a while, re-drop it
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
