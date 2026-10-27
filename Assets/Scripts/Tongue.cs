using UnityEngine;
using UnityEngine.InputSystem;

// Fires a short-range precision hit in the direction the player is holding.
// Scores by placing the ball; also used defensively to swat the ball away.
public class Tongue : MonoBehaviour
{
    [Header("Keys")]
    public Key tongueKey = Key.LeftShift;
    public Key upKey     = Key.W;
    public Key downKey   = Key.S;
    public Key leftKey   = Key.A;
    public Key rightKey  = Key.D;

    [Header("Reach")]
    public float length    = 3.5f;
    public float thickness = 0.25f;   // forgiveness when aiming at a moving ball
    public float cooldown  = 0.5f;

    [Header("Force")]
    public float ballSpeed     = 5f;  // weak on purpose: placement, not power
    public float frogKnockback = 9f;
    [Tooltip("Upward kick added to a frog knockback so the shove pops the " +
             "target off the ground instead of grinding along it.")]
    public float frogKnockbackLift = 3f;

    [Header("Refs")]
    [Tooltip("Empty child at the frog's RIGHT edge. Its X offset is mirrored " +
             "automatically when the frog faces left.")]
    public Transform mouth;
    public LineRenderer line;         // the visual
    public LayerMask hitMask;         // Ball + Frog layers
    public float showTime = 0.12f;

    FrogController frog;
    float cooldownTimer;
    float hideTimer;

    void Awake()
    {
        frog = GetComponent<FrogController>();
        if (line != null) line.enabled = false;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Hide the visual a moment after firing
        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f && line != null) line.enabled = false;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb[tongueKey].wasPressedThisFrame && cooldownTimer <= 0f)
        {
            Fire(AimDirection(kb));
        }
    }

    Vector2 AimDirection(Keyboard kb)
    {
        Vector2 dir = Vector2.zero;
        if (kb[upKey].isPressed)    dir.y += 1f;
        if (kb[downKey].isPressed)  dir.y -= 1f;
        if (kb[leftKey].isPressed)  dir.x -= 1f;
        if (kb[rightKey].isPressed) dir.x += 1f;

        // Nothing held: fire straight ahead
        if (dir == Vector2.zero) dir = new Vector2(frog.Facing, 0f);

        return dir.normalized;
    }

    void Fire(Vector2 dir)
    {
        cooldownTimer = cooldown;

        Vector2 origin = MouthOrigin();

        // The mouth sits inside this frog's own collider, so a plain CircleCast
        // would report the frog itself at distance zero every time. Cast for all
        // hits instead and take the nearest one that is not part of this frog.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            origin, thickness, dir, length, hitMask);

        Collider2D target = null;
        Rigidbody2D targetBody = null;
        float nearest = Mathf.Infinity;

        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;   // that's me

            if (h.distance < nearest)
            {
                nearest = h.distance;
                target = h.collider;
                targetBody = h.rigidbody;
            }
        }

        float drawTo = length;

        if (target != null)
        {
            drawTo = nearest;

            if (target.CompareTag("Ball") && targetBody != null)
            {
                // Setting velocity (not adding) is what makes the tongue precise:
                // the ball goes exactly where you aimed, at a known speed.
                targetBody.linearVelocity = dir * ballSpeed;
            }
            else if (target.CompareTag("Frog"))
            {
                // Shove, don't stick. Positioning tool, not a lock.
                // Goes through the other frog's controller so it can pause its
                // own movement code for a moment, otherwise the shove is
                // overwritten before it ever shows.
                FrogController other = target.GetComponent<FrogController>();
                if (other != null)
                {
                    Vector2 shove = dir * frogKnockback + Vector2.up * frogKnockbackLift;
                    other.ApplyKnockback(shove);
                }
            }
        }

        ShowTongue(origin, origin + dir * drawTo);
    }

    // The Mouth child sits on one side of the frog. Mirror its X offset so the
    // tongue always leaves from whichever side the frog is currently facing,
    // instead of starting behind the body or crossing over it.
    Vector2 MouthOrigin()
    {
        Vector3 local = mouth.localPosition;
        local.x = Mathf.Abs(local.x) * frog.Facing;
        return transform.TransformPoint(local);
    }

    void ShowTongue(Vector2 from, Vector2 to)
    {
        if (line == null) return;
        line.enabled = true;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        hideTimer = showTime;
    }
}
