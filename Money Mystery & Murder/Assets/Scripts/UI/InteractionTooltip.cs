using UnityEngine;
using TMPro;

/// <summary>
/// Displays an interaction prompt tooltip showing which key to press.
/// Can be positioned above objects or at a fixed screen position.
/// Automatically retrieves the correct key binding from KeyBindings.
/// </summary>
public class InteractionTooltip : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// The TextMeshProUGUI component that displays the tooltip text.
    /// </summary>
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Settings")]
    /// <summary>
    /// The base message to display (e.g., "to activate minigame").
    /// The key binding will be prepended automatically.
    /// </summary>
    [SerializeField] private string baseMessage = "to activate";
    
    /// <summary>
    /// Whether the tooltip should follow a world position (e.g., above an object).
    /// </summary>
    [SerializeField] private bool followWorldPosition = true;
    
    /// <summary>
    /// Offset from the target world position in screen space.
    /// </summary>
    [SerializeField] private Vector3 screenOffset = new Vector3(0, 50, 0);
    
    /// <summary>
    /// The world position to follow (set externally).
    /// </summary>
    private Vector3 _worldTarget;
    
    /// <summary>
    /// The Canvas component containing this tooltip.
    /// </summary>
    private Canvas _canvas;
    
    /// <summary>
    /// The RectTransform of this tooltip.
    /// </summary>
    private RectTransform _rectTransform;

    /// <summary>
    /// Unity lifecycle method called on initialization.
    /// Finds required components and hides the tooltip by default.
    /// </summary>
    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        
        if (tooltipText == null)
        {
            tooltipText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        Hide();
    }

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Updates the tooltip position if following a world position.
    /// </summary>
    void Update()
    {
        if (followWorldPosition && gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// Shows the tooltip with the specified action key.
    /// </summary>
    /// <param name="actionName">The name of the action (e.g., "Interact").</param>
    public void Show(string actionName = "Interact")
    {
        gameObject.SetActive(true);
        UpdateText(actionName);
    }

    /// <summary>
    /// Shows the tooltip at a specific world position.
    /// </summary>
    /// <param name="worldPosition">The world position to display the tooltip at.</param>
    /// <param name="actionName">The name of the action (e.g., "Interact").</param>
    public void Show(Vector3 worldPosition, string actionName = "Interact")
    {
        _worldTarget = worldPosition;
        Show(actionName);
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the tooltip text based on the current key binding.
    /// </summary>
    /// <param name="actionName">The name of the action to display.</param>
    private void UpdateText(string actionName)
    {
        if (tooltipText == null) return;
        
        string keyName = GetKeyName(actionName);
        tooltipText.text = $"Press [{keyName}] {baseMessage}";
    }

    /// <summary>
    /// Gets the display name of the key bound to the specified action.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <returns>The key name as a string.</returns>
    private string GetKeyName(string actionName)
    {
        var bindings = KeyBindings.Instance;
        if (bindings == null) return "E"; // Fallback
        
        switch (actionName)
        {
            case "Interact":
                return bindings.Interact.ToString();
            case "OpenShop":
                return bindings.OpenShop.ToString();
            case "UseItem":
                return bindings.UseItem.ToString();
            default:
                return "E"; // Fallback
        }
    }

    /// <summary>
    /// Updates the tooltip position to follow the world target.
    /// </summary>
    private void UpdatePosition()
    {
        if (_canvas == null || _rectTransform == null) return;
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Convert world position to screen position
        Vector3 screenPos = cam.WorldToScreenPoint(_worldTarget);
        
        // Check if behind camera
        if (screenPos.z < 0)
        {
            Hide();
            return;
        }
        
        // Apply offset
        screenPos += screenOffset;
        
        // Convert screen position to canvas position
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _rectTransform.position = screenPos;
        }
        else if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, 
                screenPos, 
                _canvas.worldCamera, 
                out canvasPos
            );
            _rectTransform.localPosition = canvasPos;
        }
    }

    /// <summary>
    /// Sets the world position for the tooltip to follow.
    /// </summary>
    /// <param name="worldPosition">The world position to follow.</param>
    public void SetWorldTarget(Vector3 worldPosition)
    {
        _worldTarget = worldPosition;
    }

    /// <summary>
    /// Sets the base message displayed in the tooltip.
    /// </summary>
    /// <param name="message">The base message (e.g., "to activate minigame").</param>
    public void SetBaseMessage(string message)
    {
        baseMessage = message;
        if (gameObject.activeSelf)
        {
            UpdateText("Interact");
        }
    }
}
