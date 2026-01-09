using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes UI elements always visible regardless of lighting (e.g., minimap).
/// UI Canvas should use Screen Space overlay mode to be unaffected by lighting.
/// </summary>
public class AlwaysVisibleUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Tooltip("Automatically configure canvas on Start")]
    private bool autoConfigureCanvas = true;
    
    private Canvas _canvas;
    
    void Start()
    {
        if (autoConfigureCanvas)
        {
            ConfigureCanvas();
        }
    }
    
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
