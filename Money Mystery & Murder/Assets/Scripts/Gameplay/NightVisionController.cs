using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls player vision during nighttime by limiting visibility range and darkening the view.
/// Automatically adjusts Light2D intensity and range based on current game phase.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class NightVisionController : MonoBehaviour
{
    [Header("Night Vision Settings")]
    [SerializeField] [Tooltip("Vision range during day (normal visibility)")]
    private float dayVisionRange = 25f;
    
    [SerializeField] [Tooltip("Vision range during evening (reduced visibility)")]
    private float eveningVisionRange = 15f;
    
    [SerializeField] [Tooltip("Vision range during night (limited visibility - Among Us style)")]
    private float nightVisionRange = 12f;
    
    [Header("Light Intensity Settings")]
    [SerializeField] [Tooltip("Light intensity during day")]
    private float dayIntensity = 1.5f;
    
    [SerializeField] [Tooltip("Light intensity during evening")]
    private float eveningIntensity = 1.0f;
    
    [SerializeField] [Tooltip("Light intensity during night (darker for atmosphere)")]
    private float nightIntensity = 0.5f;
    
    [Header("Transition Settings")]
    [SerializeField] [Tooltip("How smoothly the vision transitions between phases")]
    private float transitionSpeed = 3f;
    
    [Header("Light Falloff (Advanced)")]
    [SerializeField] [Tooltip("Inner radius percentage (0-1) where light is at full intensity")]
    [Range(0f, 1f)]
    private float innerRadiusMultiplier = 0.3f;
    
    [SerializeField] [Tooltip("Use distance falloff for more realistic lighting")]
    private bool useDistanceFalloff = true;
    
    [Header("Bot Settings")]
    [SerializeField] [Tooltip("If false, this object won't have light (for bots/NPCs)")]
    private bool enableLightForThisObject = true;
    
    private Light2D _playerLight;
    private GamePhase _currentPhase = GamePhase.Day;
    private float _targetRange;
    private float _targetIntensity;
    
    void Awake()
    {
        // Check if this is a bot (name contains "BOT") or doesn't have Player component with player index 0
        bool isBot = gameObject.name.Contains("BOT");
        Player player = GetComponent<Player>();
        
        // If it's a bot, disable light and don't initialize
        if (isBot || !enableLightForThisObject)
        {
            _playerLight = GetComponent<Light2D>();
            if (_playerLight != null)
            {
                DestroyImmediate(_playerLight);
                Debug.Log($"[NightVisionController] Removed Light2D from bot/NPC: {gameObject.name}");
            }
            enabled = false;
            return;
        }
        
        _playerLight = GetComponent<Light2D>();
        
        if (_playerLight == null)
        {
            _playerLight = gameObject.AddComponent<Light2D>();
            _playerLight.lightType = Light2D.LightType.Point;
            Debug.Log($"[NightVisionController] Added Light2D to player: {gameObject.name}");
        }
        
        // Initialize with day settings
        _targetRange = dayVisionRange;
        _targetIntensity = dayIntensity;
        _playerLight.pointLightOuterRadius = _targetRange;
        _playerLight.pointLightInnerRadius = _targetRange * innerRadiusMultiplier;
        _playerLight.intensity = _targetIntensity;
        
        // Configure falloff for better visibility limitation
        if (useDistanceFalloff)
        {
            _playerLight.falloffIntensity = 1.0f; // Strong falloff
        }
        
        Debug.Log($"[NightVisionController] Initialized on {gameObject.name}. Day range: {dayVisionRange}, Night range: {nightVisionRange}");
    }
    
    void Update()
    {
        UpdateVisionBasedOnPhase();
        SmoothTransition();
    }
    
    /// <summary>
    /// Updates target vision settings based on current game phase.
    /// </summary>
    private void UpdateVisionBasedOnPhase()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[NightVisionController] GameManager.Instance is null!");
            return;
        }
        
        GamePhase currentPhase = GameManager.Instance.CurrentPhase;
        
        // Only update if phase changed
        if (currentPhase == _currentPhase) return;
        
        GamePhase previousPhase = _currentPhase;
        _currentPhase = currentPhase;
        
        switch (_currentPhase)
        {
            case GamePhase.Day:
                _targetRange = dayVisionRange;
                _targetIntensity = dayIntensity;
                break;
                
            case GamePhase.Evening:
                _targetRange = eveningVisionRange;
                _targetIntensity = eveningIntensity;
                break;
                
            case GamePhase.Night:
                _targetRange = nightVisionRange;
                _targetIntensity = nightIntensity;
                break;
                
            case GamePhase.End:
            case GamePhase.Lobby:
                _targetRange = dayVisionRange;
                _targetIntensity = dayIntensity;
                break;
        }
        
        Debug.Log($"[NightVisionController] {gameObject.name} Phase {previousPhase} → {_currentPhase}. Target range: {_targetRange}, Target intensity: {_targetIntensity}");
    }
    
    /// <summary>
    /// Smoothly transitions light properties to target values.
    /// </summary>
    private void SmoothTransition()
    {
        if (_playerLight == null) return;
        
        // Smoothly interpolate to target values
        _playerLight.pointLightOuterRadius = Mathf.Lerp(
            _playerLight.pointLightOuterRadius,
            _targetRange,
            Time.deltaTime * transitionSpeed
        );
        
        _playerLight.pointLightInnerRadius = Mathf.Lerp(
            _playerLight.pointLightInnerRadius,
            _targetRange * innerRadiusMultiplier,
            Time.deltaTime * transitionSpeed
        );
        
        _playerLight.intensity = Mathf.Lerp(
            _playerLight.intensity,
            _targetIntensity,
            Time.deltaTime * transitionSpeed
        );
    }
    
    /// <summary>
    /// Gets the current vision range.
    /// </summary>
    public float CurrentVisionRange => _playerLight != null ? _playerLight.pointLightOuterRadius : dayVisionRange;
    
    /// <summary>
    /// Gets the current light intensity.
    /// </summary>
    public float CurrentIntensity => _playerLight != null ? _playerLight.intensity : dayIntensity;
    
    /// <summary>
    /// Manually sets the vision range (e.g., for abilities or items).
    /// </summary>
    /// <param name="range">The new vision range.</param>
    public void SetVisionRange(float range)
    {
        _targetRange = range;
    }
    
    /// <summary>
    /// Manually sets the light intensity (e.g., for abilities or items).
    /// </summary>
    /// <param name="intensity">The new light intensity.</param>
    public void SetIntensity(float intensity)
    {
        _targetIntensity = Mathf.Clamp01(intensity);
    }
    
    /// <summary>
    /// Temporarily boosts vision (e.g., for night vision ability).
    /// </summary>
    /// <param name="rangeMultiplier">Multiplier for vision range.</param>
    /// <param name="intensityBoost">Additional intensity.</param>
    public void BoostVision(float rangeMultiplier = 2f, float intensityBoost = 0.5f)
    {
        _targetRange *= rangeMultiplier;
        _targetIntensity = Mathf.Clamp01(_targetIntensity + intensityBoost);
    }
    
    /// <summary>
    /// Resets vision to default phase values.
    /// </summary>
    public void ResetToPhaseDefaults()
    {
        UpdateVisionBasedOnPhase();
    }
}
