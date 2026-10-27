using UnityEngine;

// A trigger sitting just below a gap in the lily pad.
// The ball falling through it scores for the OTHER player.
public class Hole : MonoBehaviour
{
    [Tooltip("Which player earns a point when the ball drops through this hole.")]
    public int scoringPlayer = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        GameManager.Instance.ScorePoint(scoringPlayer);
    }
}
