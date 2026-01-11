using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Manages the main menu UI functionality including scene transitions and music.
    /// Handles starting the game and navigating to settings.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>
        /// Audio clip to play as background music in the main menu.
        /// </summary>
        public AudioClip menuMusic;

        /// <summary>
        /// Unity lifecycle method called before the first frame update.
        /// Starts playing the menu music if available.
        /// </summary>
        void Start()
        {
            if (AudioManager.Instance != null && menuMusic != null)
            {
                AudioManager.Instance.PlayMusic(menuMusic);
            }
        }

        /// <summary>
        /// Starts the game by loading the gameplay scene.
        /// Called by UI button click events.
        /// </summary>
        public void StartGame()
        {
            SceneManager.LoadScene("GamePlay");
        }

        /// <summary>
        /// Opens the settings menu by loading the settings scene.
        /// Called by UI button click events.
        /// </summary>
        public void OpenSettings()
        {
            SceneManager.LoadScene("SettingsScene");
        }
    }
}