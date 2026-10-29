using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Owns the score, the round reset, and the end of the match.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // match
    public int pointsToWin = 5;
    public Key restartKey = Key.R;

    public Ball ball;
    public Transform frog1;
    public Transform frog2;
    public TextMeshProUGUI score1Text;
    public TextMeshProUGUI score2Text;
    public TextMeshProUGUI winnerText;   // hidden until someone wins

    // audio
    public AudioSource audioSource;
    public AudioClip splashClip;
    public AudioClip winClip;

    int score1;
    int score2;
    Vector3 frog1Start;
    Vector3 frog2Start;
    bool matchOver;

    void Awake()
    {
        Instance = this;
        frog1Start = frog1.position;
        frog2Start = frog2.position;
    }

    void Start()
    {
        if (winnerText != null) winnerText.gameObject.SetActive(false);
        UpdateScoreText();
    }

    void Update()
    {
        // input; restarting when someone wins
        if (!matchOver) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb[restartKey].wasPressedThisFrame) RestartMatch();
    }

    public void ScorePoint(int player)
    {
        if (matchOver) return;

        if (audioSource != null && splashClip != null) audioSource.PlayOneShot(splashClip);

        if (player == 1) score1++; else score2++;
        UpdateScoreText();

        if (score1 >= pointsToWin || score2 >= pointsToWin)
        {
            EndMatch(score1 >= pointsToWin ? 1 : 2);
        }
        else
        {
            ResetRound();
        }
    }

    void ResetRound()
    {
        frog1.position = frog1Start;
        frog2.position = frog2Start;

        ZeroVelocity(frog1);
        ZeroVelocity(frog2);

        ResetFacing(frog1);
        ResetFacing(frog2);

        ball.ResetToCenter();
    }

    void ZeroVelocity(Transform t)
    {
        Rigidbody2D rb = t.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void ResetFacing(Transform t)
    {
        FrogController frog = t.GetComponent<FrogController>();
        if (frog != null) frog.ResetFacing();
    }

    void EndMatch(int winner)
    {
        if (audioSource != null && winClip != null) audioSource.PlayOneShot(winClip);
        
        matchOver = true;
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = "Player " + winner + " wins!\nPress R to play again";
        }
        ball.gameObject.SetActive(false);
    }

    void RestartMatch()
    {
        matchOver = false;
        score1 = 0;
        score2 = 0;
        UpdateScoreText();

        if (winnerText != null) winnerText.gameObject.SetActive(false);

        ball.gameObject.SetActive(true);
        ResetRound();
    }

    void UpdateScoreText()
    {
        if (score1Text != null) score1Text.text = score1.ToString();
        if (score2Text != null) score2Text.text = score2.ToString();
    }
}
