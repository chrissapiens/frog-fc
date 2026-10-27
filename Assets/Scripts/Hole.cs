using UnityEngine;

// Trigger under the holes in the lily pad
// The ball falling through it scores for the other player
public class Hole : MonoBehaviour
{
    public int scoringPlayer = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        GameManager.Instance.ScorePoint(scoringPlayer);
    }
}
