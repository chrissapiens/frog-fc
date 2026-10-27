using UnityEngine;
using UnityEngine.InputSystem;

// Fires a short-range precision hit in the direction the player is holding.
// Scores by placing the ball; also used defensively to swat the ball away.
public class Tongue : MonoBehaviour
{
    // keys
    public Key tongueKey = Key.LeftShift;
    public Key upKey     = Key.W;
    public Key downKey   = Key.S;
    public Key leftKey   = Key.A;
    public Key rightKey  = Key.D;

    // reach
    public float length    = 3.5f;
    public float thickness = 0.25f;   // forgiveness when aiming at a moving ball
    public float cooldown  = 0.5f;

    // strength
    public float ballSpeed     = 5f;  // weak on purpose; for placement
    public float frogKnockback = 9f;
    public float frogKnockbackLift = 3f;

    // empty child at the frog's right edge, mirroreed when facing left
    public Transform mouth;
    public LineRenderer line;         // the visual
    public LayerMask hitMask;         // ball and frog layers
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

        // hide visual a moment after firing
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

        // fire straight ahead
        if (dir == Vector2.zero) dir = new Vector2(frog.Facing, 0f);

        return dir.normalized;
    }

    void Fire(Vector2 dir)
    {
        cooldownTimer = cooldown;

        Vector2 origin = MouthOrigin();

        // the mouth sits inside this frog's own collider, so a single cast would hit the frog itself
        // cast for all hits and take the nearest one that isn't it
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            origin, thickness, dir, length, hitMask);

        Collider2D target = null;
        Rigidbody2D targetBody = null;
        float nearest = Mathf.Infinity;

        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;   // this frog, not a target

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
                // setting velocity makes the tongue precise
                // the ball goes exactly where you aimed, at a known speed
                targetBody.linearVelocity = dir * ballSpeed;
            }
            else if (target.CompareTag("Frog"))
            {
                // push the other frog instead of grabbing them
                // goes through their controller so the shove isn't immediately overwritten by their movement code
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

    // the mouth is fixed to one side, so flip its X to match the frog's facing
    // otherwise the tongue fires from behind the frog when it turns around.
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
