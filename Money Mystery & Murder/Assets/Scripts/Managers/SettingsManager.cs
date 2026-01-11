using UnityEngine;

/// <summary>
/// Manages game settings such as volume and fullscreen mode.
/// Persists settings using PlayerPrefs and ensures singleton pattern.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the SettingsManager.
    /// </summary>
    public static SettingsManager Instance { get; private set; }

    /// <summary>
    /// PlayerPrefs key for storing the game volume setting.
    /// </summary>
    private const string VolumeKey = "GameVolume";
    
    /// <summary>
    /// PlayerPrefs key for storing the fullscreen mode setting.
    /// </summary>
    private const string FullscreenKey = "FullscreenMode";

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Ensures only one instance exists and persists across scene loads.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Loads saved settings from PlayerPrefs.
    /// </summary>
    void Start()
    {
        LoadSettings();
    }

    /// <summary>
    /// Sets the game volume and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="volume">The volume level to set (0.0 to 1.0).</param>
    public void SetVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(volume);
        }
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets the fullscreen mode and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="isFullscreen">True to enable fullscreen mode, false for windowed mode.</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads saved settings from PlayerPrefs and applies them.
    /// Uses default values if no saved settings exist.
    /// </summary>
    private void LoadSettings()
    {
        // Load volume
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1.0f); // Default to 1.0
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(volume);
        }

        // Load fullscreen
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1; // Default to fullscreen
        Screen.fullScreen = isFullscreen;
    }
}