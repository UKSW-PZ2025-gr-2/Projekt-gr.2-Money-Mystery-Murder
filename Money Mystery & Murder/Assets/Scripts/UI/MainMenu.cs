using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioClip menuMusic;

    void Start()
    {
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }
}