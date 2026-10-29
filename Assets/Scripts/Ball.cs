using UnityEngine;

// - caps the ball's top speed
// - the ball recovers if it escapes, gets stuck, or is sitting still
public class Ball : MonoBehaviour
{
    public float maxSpeed = 18f;

    public float killY = -12f;            // fell out of the world
    public float stuckSpeed = 0.1f;       // ball is stuck
    public float stuckSeconds = 15f;      // how long before we intervene

    public int maxCharge = 3; // how many pips can be held
    public float gravityPerPip = 0.5f; // heavier ball for full charge
    public float speedLossPerPip = 2f; // heavier ball is slower

    // squeeze escape
    public float squeezeLift = 1.5f;
    public float squeezeEscape = 4f;
    public float squeezeSpeed = 1.5f;

    // time for charge to disappear
    public float chargeDecaySeconds = 3f;

    Rigidbody2D rb;
    Vector2 startPos;
    float stuckTimer;
    int charge;
    float decayTimer;
    float baseGravity;
    float escapeTimer;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravity  = rb.gravityScale;
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        // FixedUpdate runs on the physics clock; the cap applies to every step
        float cap = CurrentMaxSpeed();
        if (rb.linearVelocity.magnitude > cap)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * cap;
        }
    }

    void Update()
    {
        // charge dies down
        if (charge > 0)
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= chargeDecaySeconds)
            {
                decayTimer = 0f;
                charge--;
                ApplyCharge();
            }
        }

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

        charge = 0;
        decayTimer = 0f;
        ApplyCharge();
    }

    // read by the HUD bar every frame
    public int Charge { get { return charge; } }

    // called by a Tongue that just hit this ball
    public void AddCharge()
    {
        decayTimer = 0f;        // a fresh hit restarts the clock, even at full
        if (charge >= maxCharge) return;
        charge++;
        ApplyCharge();
    }

    // pushes the current charge into the physics values
    void ApplyCharge()
    {
        rb.gravityScale = baseGravity + gravityPerPip * charge;
    }

    // - speed cap shrinks as the ball charges up
    // - fully charged ball still moves
    float CurrentMaxSpeed()
    {
        return Mathf.Max(5f, maxSpeed - speedLossPerPip * charge);
    }

    // ball trapped between two frogs will pop up
    void OnCollisionStay2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Frog")) return;
        if (rb.linearVelocity.magnitude > squeezeSpeed) return;
        if (Time.time < escapeTimer) return;

        escapeTimer = Time.time + 0.5f;

        // the ball has to be moved clear of the pile before it is given any speed
        rb.position = rb.position + Vector2.up * squeezeLift;
        rb.linearVelocity = Vector2.up * squeezeEscape;
    }
}

