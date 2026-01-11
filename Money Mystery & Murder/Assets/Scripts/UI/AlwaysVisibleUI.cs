using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes UI elements always visible regardless of lighting (e.g., minimap).
/// UI Canvas should use Screen Space overlay mode to be unaffected by lighting.
/// </summary>
public class AlwaysVisibleUI : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>
    /// Automatically configure canvas on Start to ensure always-visible rendering.
    /// </summary>
    [SerializeField] [Tooltip("Automatically configure canvas on Start")]
    private bool autoConfigureCanvas = true;
    
    /// <summary>
    /// Cached Canvas component for this UI element.
    /// </summary>
    private Canvas _canvas;
    
    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Configures the canvas if autoConfigureCanvas is enabled.
    /// </summary>
    void Start()
    {
        if (autoConfigureCanvas)
        {
            ConfigureCanvas();
        }
    }
    
    /// <summary>
    /// Configures the Canvas component to render in Screen Space Overlay mode
    /// with high sorting order for always-visible display.
    /// </summary>
    [ContextMenu("Configure Canvas for Always Visible")]
    public void ConfigureCanvas()
    {
        _canvas = GetComponent<Canvas>();
        
        if (_canvas == null)
        {
            Debug.LogWarning($"[AlwaysVisibleUI] No Canvas component found on {gameObject.name}");
            return;
        }
        
        // Set to Screen Space - Overlay so it's not affected by lighting
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Ensure it's on top
        _canvas.sortingOrder = 999;
        
        Debug.Log($"[AlwaysVisibleUI] Configured {gameObject.name} to be always visible");
    }
}
