using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }
}