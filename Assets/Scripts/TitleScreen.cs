using UnityEngine;
using UnityEngine.InputSystem;

// - title screen before match starts
// - this freezes the game with timeScale instead of loading a separate scene
// - script lives on its own object because a disabled object stops running Update and would never see the start key
public class TitleScreen : MonoBehaviour
{
    public GameObject panel;
    public GameObject[] hideWhileShowing;
    public Key startKey = Key.Space;

    void Start()
    {
        Show();
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb[startKey].wasPressedThisFrame) Hide();
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
        SetHud(false);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        SetHud(true);
        Time.timeScale = 1f;
    }

    void SetHud(bool on)
    {
        foreach (GameObject go in hideWhileShowing)
        {
            if (go != null) go.SetActive(on);
        }
    }
}