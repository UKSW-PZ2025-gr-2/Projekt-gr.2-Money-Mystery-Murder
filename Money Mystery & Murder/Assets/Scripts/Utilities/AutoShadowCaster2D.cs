using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Automatically adds Shadow Caster 2D component to sprites to make them invisible in darkness.
/// This ensures objects are only visible when illuminated by player lights during night.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AutoShadowCaster2D : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] [Tooltip("If true, automatically add ShadowCaster2D on Start")]
    private bool autoAddShadowCaster = true;
    
    [SerializeField] [Tooltip("If true, cast shadows in all directions")]
    private bool castsShadows = true;
    
    [SerializeField] [Tooltip("If true, object itself will be affected by shadows")]
    private bool selfShadows = false;
    
    [Header("Material Settings")]
    [SerializeField] [Tooltip("If true, automatically set sprite to use Lit material")]
    private bool autoSetLitMaterial = true;
    
    private SpriteRenderer _spriteRenderer;
    private ShadowCaster2D _shadowCaster;
    private LitSpriteSetup _litSetup;
    
    void Start()
    {
        if (!autoAddShadowCaster) return;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            Debug.LogWarning($"[AutoShadowCaster2D] No SpriteRenderer found on {gameObject.name}");
            return;
        }
        
        // Check if ShadowCaster2D already exists
        _shadowCaster = GetComponent<ShadowCaster2D>();
        
        if (_shadowCaster == null)
        {
            // Add ShadowCaster2D component
            _shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
            Debug.Log($"[AutoShadowCaster2D] Added ShadowCaster2D to {gameObject.name}");
        }
        
        // Configure shadow caster
        _shadowCaster.castsShadows = castsShadows;
        _shadowCaster.selfShadows = selfShadows;
        
        // Setup Lit material if enabled
        if (autoSetLitMaterial)
        {
            _litSetup = GetComponent<LitSpriteSetup>();
            if (_litSetup == null)
            {
                _litSetup = gameObject.AddComponent<LitSpriteSetup>();
            }
            _litSetup.SetupLitSprite();
        }
    }
    
    /// <summary>
    /// Toggles shadow casting on/off.
    /// </summary>
    public void SetCastsShadows(bool enabled)
    {
        castsShadows = enabled;
        if (_shadowCaster != null)
        {
            _shadowCaster.castsShadows = enabled;
        }
    }
}
