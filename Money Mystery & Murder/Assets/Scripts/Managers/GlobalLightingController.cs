using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls global lighting based on game phase (day/evening/night).
/// Adjusts Global Light 2D intensity to create darker atmosphere during night.
/// </summary>
public class GlobalLightingController : MonoBehaviour
{
    [Header("Global Light Reference")]
    [SerializeField] [Tooltip("Reference to the main Global Light 2D in the scene")]
    private Light2D globalLight;
    
    [Header("Light Intensity by Phase")]
    [SerializeField] [Tooltip("Global light intensity during day")]
    private float dayLightIntensity = 1.0f;
    
    [SerializeField] [Tooltip("Global light intensity during evening")]
    private float eveningLightIntensity = 0.4f;
    
    [SerializeField] [Tooltip("Global light intensity during night - set to 0 for complete darkness")]
    private float nightLightIntensity = 0.0f;
    
    [Header("Ambient Color by Phase")]
    [SerializeField] [Tooltip("Light color during day")]
    private Color dayColor = new Color(1f, 1f, 1f, 1f);
    
    [SerializeField] [Tooltip("Light color during evening")]
    private Color eveningColor = new Color(1f, 0.7f, 0.4f, 1f);
    
    [SerializeField] [Tooltip("Light color during night - COMPLETE BLACK for Among Us effect")]
    private Color nightColor = new Color(0f, 0f, 0f, 1f); // Całkowita ciemność
    
    [Header("Transition Settings")]
    [SerializeField] [Tooltip("Speed of lighting transitions")]
    private float transitionSpeed = 2f;
    
    [Header("Among Us Mode")]
    [SerializeField] [Tooltip("Force complete darkness at night (0 intensity, black color)")]
    private bool forceCompleteBlackness = true;
    
    private GamePhase _currentPhase = GamePhase.Day;
    private float _targetIntensity;
    private Color _targetColor;
    
    void Awake()
    {
        // Try to find global light if not assigned
        if (globalLight == null)
        {
            globalLight = FindGlobalLight();
        }
        
        if (globalLight == null)
        {
            Debug.LogWarning("[GlobalLightingController] No Global Light 2D found in scene. Global lighting effects will not work.");
            enabled = false;
            return;
        }
        
        // Initialize with day settings
        _targetIntensity = dayLightIntensity;
        _targetColor = dayColor;
        globalLight.intensity = _targetIntensity;
        globalLight.color = _targetColor;
    }
    
    void Update()
    {
        UpdateLightingBasedOnPhase();
        SmoothTransition();
    }
    
    /// <summary>
    /// Finds the Global Light 2D in the scene.
    /// </summary>
    private Light2D FindGlobalLight()
    {
        Light2D[] lights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        
        foreach (Light2D light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                Debug.Log($"[GlobalLightingController] Found Global Light: {light.gameObject.name}");
                return light;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Updates target lighting settings based on current game phase.
    /// </summary>
    private void UpdateLightingBasedOnPhase()
    {
        if (GameManager.Instance == null) return;
        
        GamePhase currentPhase = GameManager.Instance.CurrentPhase;
        
        // Only update if phase changed
        if (currentPhase == _currentPhase) return;
        
        _currentPhase = currentPhase;
        
        switch (_currentPhase)
        {
            case GamePhase.Day:
                _targetIntensity = dayLightIntensity;
                _targetColor = dayColor;
                break;
                
            case GamePhase.Evening:
                _targetIntensity = eveningLightIntensity;
                _targetColor = eveningColor;
                break;
                
            case GamePhase.Night:
                if (forceCompleteBlackness)
                {
                    _targetIntensity = 0f; // Całkowita ciemność
                    _targetColor = Color.black; // Czarny kolor
                }
                else
                {
                    _targetIntensity = nightLightIntensity;
                    _targetColor = nightColor;
                }
                break;
                
            case GamePhase.End:
            case GamePhase.Lobby:
                _targetIntensity = dayLightIntensity;
                _targetColor = dayColor;
                break;
        }
        
        Debug.Log($"[GlobalLightingController] Phase changed to {_currentPhase}. Target intensity: {_targetIntensity}");
    }
    
    /// <summary>
    /// Smoothly transitions lighting properties to target values.
    /// </summary>
    private void SmoothTransition()
    {
        if (globalLight == null) return;
        
        globalLight.intensity = Mathf.Lerp(
            globalLight.intensity,
            _targetIntensity,
            Time.deltaTime * transitionSpeed
        );
        
        globalLight.color = Color.Lerp(
            globalLight.color,
            _targetColor,
            Time.deltaTime * transitionSpeed
        );
    }
    
    /// <summary>
    /// Manually sets the global light intensity.
    /// </summary>
    public void SetIntensity(float intensity)
    {
        _targetIntensity = Mathf.Clamp01(intensity);
    }
    
    /// <summary>
    /// Manually sets the global light color.
    /// </summary>
    public void SetColor(Color color)
    {
        _targetColor = color;
    }
}
