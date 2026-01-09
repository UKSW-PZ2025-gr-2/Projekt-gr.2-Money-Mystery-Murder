using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public Button closeButton;
    public Button controlsButton;
    
    [Header("Panels")]
    public GameObject keyRebindPanel;

    void Start()
    {
        // Hide controls panel by default
        if (keyRebindPanel != null)
            keyRebindPanel.SetActive(false);
            
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
        
        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(OpenControlsPanel);
        }
    }
    
    public void OpenControlsPanel()
    {
        if (keyRebindPanel != null)
        {
            keyRebindPanel.SetActive(true);
            // Hide main settings panel
            gameObject.SetActive(false);
        }
    }
    
    public void CloseControlsPanel()
    {
        if (keyRebindPanel != null)
            keyRebindPanel.SetActive(false);
        
        // Show main settings panel again
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}