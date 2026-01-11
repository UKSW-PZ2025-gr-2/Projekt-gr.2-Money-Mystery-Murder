using UnityEngine;

/// <summary>
/// Automatically configures SpriteRenderer to use Lit material so sprites are only visible when illuminated.
/// This makes objects (players, enemies, items) invisible in complete darkness and visible only in light.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class LitSpriteSetup : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>
    /// Automatically setup lit material on Start if enabled.
    /// </summary>
    [SerializeField] [Tooltip("Automatically setup lit material on Start")]
    private bool autoSetupOnStart = true;
    
    /// <summary>
    /// Material to use for lighting. Leave empty to find default Sprite-Lit-Default material.
    /// </summary>
    [SerializeField] [Tooltip("Material to use (leave empty to find default Sprite-Lit-Default)")]
    private Material litMaterial;
    
    [Header("Advanced")]
    /// <summary>
    /// Force sprite color to white for proper lighting response. Disabled by default to preserve original colors.
    /// </summary>
    [SerializeField] [Tooltip("Force sprite color to white for proper lighting")]
    private bool forceWhiteColor = false;
    
    /// <summary>
    /// Cached SpriteRenderer component.
    /// </summary>
    private SpriteRenderer _spriteRenderer;
    
    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Automatically configures lit sprite if autoSetupOnStart is enabled.
    /// </summary>
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLitSprite();
        }
    }
    
    /// <summary>
    /// Configures the SpriteRenderer to use a lit material for proper lighting response.
    /// Can be called manually or from the context menu.
    /// </summary>
    [ContextMenu("Setup Lit Sprite")]
    public void SetupLitSprite()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            Debug.LogError($"[LitSpriteSetup] No SpriteRenderer found on {gameObject.name}");
            return;
        }
        
        // If no material assigned, try to find the default Sprite-Lit-Default
        if (litMaterial == null)
        {
            litMaterial = FindLitMaterial();
        }
        
        if (litMaterial != null)
        {
            _spriteRenderer.sharedMaterial = litMaterial; // Use sharedMaterial to avoid creating instances
            Debug.Log($"[LitSpriteSetup] Set lit material on {gameObject.name}");
        }
        else
        {
            // Fallback: create material with Sprite-Lit-Default shader
            Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (litShader != null)
            {
                Material newMat = new Material(litShader);
                _spriteRenderer.sharedMaterial = newMat; // Use sharedMaterial
                Debug.Log($"[LitSpriteSetup] Created new Sprite-Lit-Default material for {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[LitSpriteSetup] Could not find Sprite-Lit-Default shader! Make sure URP is properly configured.");
            }
        }
        
        // Force white color for proper lighting response
        if (forceWhiteColor && _spriteRenderer.color != Color.white)
        {
            _spriteRenderer.color = Color.white;
            Debug.Log($"[LitSpriteSetup] Set sprite color to white on {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Searches for an existing Sprite-Lit material in the project resources.
    /// </summary>
    /// <returns>The found lit material, or null if none exists.</returns>
    private Material FindLitMaterial()
    {
        // Try to find existing Sprite-Lit-Default material in Resources or project
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
        
        foreach (Material mat in allMaterials)
        {
            if (mat.shader != null && mat.shader.name.Contains("Sprite-Lit"))
            {
                return mat;
            }
        }
        
        return null;
    }
}
