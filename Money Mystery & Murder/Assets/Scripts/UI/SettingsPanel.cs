using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public Button closeButton;

    void Start()
    {
        // Initialize values first (use current values if SettingsManager not ready)
        if (SettingsManager.Instance != null)
        {
            volumeSlider.value = AudioListener.volume;
            fullscreenToggle.isOn = Screen.fullScreen;
        }
        else
        {
            // Fallback: set defaults
            volumeSlider.value = 1.0f;
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        // Add listeners after ensuring Instance exists
        if (volumeSlider != null && SettingsManager.Instance != null)
        {
            volumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetVolume);
        }

        if (fullscreenToggle != null && SettingsManager.Instance != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SettingsManager.Instance.SetFullscreen);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void ClosePanel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}