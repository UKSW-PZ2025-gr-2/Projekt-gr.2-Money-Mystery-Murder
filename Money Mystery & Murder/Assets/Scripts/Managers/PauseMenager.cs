using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuPanel; // panel pauzy w Canvas
    private bool isPaused = false;

    void Start()
    {
        // Na start menu pauzy ukryte
        if(menuPanel != null)
            menuPanel.SetActive(true);
    }

    void Update()
    {
        // Naciśnięcie ESC
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if(menuPanel != null)
            menuPanel.SetActive(isPaused);

        // Zatrzymanie gry (animacje, AI, fizyka) jeśli chcesz
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if(menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
