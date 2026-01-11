using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the game menu, including starting the game and exiting the application.
/// Note: The filename is PauseMenager.cs but the class is named MenuManager.
/// </summary>
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// The menu panel GameObject that is shown/hidden when starting the game.
    /// </summary>
    public GameObject menuPanel;


    /// <summary>
    /// Starts the game by hiding the menu panel and resuming time.
    /// </summary>
    public void StartGame()
    {

        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Exits the game application.
    /// In the Unity Editor, this stops play mode instead of quitting.
    /// </summary>
   public void ExitGame()
    {

    Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }


}


