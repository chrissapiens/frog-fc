using UnityEngine;
using UnityEngine.InputSystem;

// - fires short hit in the direction the player is holding
// - score by placing the ball; can also swat ball away
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
    public Transform tongueSprite;   // the tongue art in the scene
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
        if (tongueSprite != null) tongueSprite.gameObject.SetActive(false);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // hide visual a moment after firing
        if (hideTimer > 0f)
        {
            // - keep the tongue stuck to the mouth while it is out
            // - doesn't get left behind when the frog moves mid-fire
            if (tongueSprite != null) tongueSprite.position = MouthOrigin();

            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f && line != null) line.enabled = false;
            if (hideTimer <= 0f && tongueSprite != null) tongueSprite.gameObject.SetActive(false);
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

        // - the mouth sits inside this frog's own collider, so a single cast would hit the frog itself
        // - cast for all hits and take the nearest one that isn't it
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
                // - setting velocity makes the tongue precise
                // - the ball goes exactly where you aimed, at a known speed
                targetBody.linearVelocity = dir * ballSpeed;
            }
            else if (target.CompareTag("Frog"))
            {
                // - push the other frog instead of grabbing them
                // - goes through their controller so the shove isn't overwritten by their movement code
                FrogController other = target.GetComponent<FrogController>();
                if (other != null)
                {
                    Vector2 shove = dir * frogKnockback + Vector2.up * frogKnockbackLift;
                    other.ApplyKnockback(shove);
                }
            }
        }

        ShowTongue(origin, origin + dir * drawTo);
        if (frog != null) frog.ShowTongueSprite(showTime);

    }

    // - the mouth is fixed to one side, so flip its X to match the frog's facing
    // - otherwise the tongue fires from behind the frog when it turns around.
    Vector2 MouthOrigin()
    {
        Vector3 local = mouth.localPosition;
        local.x = Mathf.Abs(local.x) * frog.Facing;
        return transform.TransformPoint(local);
    }

    // - points the tongue art from the mouth to wherever cast ended
    // - X scale is distance it needs to cover 
    // - stops short on a hit instead of drawing at full reach every time
    void ShowTongue(Vector2 from, Vector2 to)
    {
        if (tongueSprite == null) return;

        Vector2 delta = to - from;

        tongueSprite.gameObject.SetActive(true);
        tongueSprite.position = from;
        tongueSprite.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        tongueSprite.localScale = new Vector3(delta.magnitude, 1f, 1f);

        hideTimer = showTime;
    }
}
