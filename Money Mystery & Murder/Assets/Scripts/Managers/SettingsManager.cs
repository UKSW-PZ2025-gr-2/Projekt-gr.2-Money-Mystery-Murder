using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string VolumeKey = "GameVolume";
    private const string FullscreenKey = "FullscreenMode";

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

    void Start()
    {
        LoadSettings();
    }

    public void SetVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(volume);
        }
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

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