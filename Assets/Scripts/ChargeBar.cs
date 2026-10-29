using UnityEngine;
using UnityEngine.UI;

// - three pips between the scores, showing how much charge the ball holds
public class ChargeBar : MonoBehaviour
{
    // refs
    public Ball ball;
    public Image[] pips;

    // sprites
    public Sprite fullPip;
    public Sprite emptyPip;

    // last value drawn, starts at -1 so the first frame always draws
    int lastCharge = -1;

    void Update()
    {
        if (ball == null) return;

        int charge = ball.Charge;
        if (charge == lastCharge) return;   // nothing changed, skip the work
        lastCharge = charge;

        for (int i = 0; i < pips.Length; i++)
        {
            if (pips[i] == null) continue;
            pips[i].sprite = (i < charge) ? fullPip : emptyPip;
        }
    }
}