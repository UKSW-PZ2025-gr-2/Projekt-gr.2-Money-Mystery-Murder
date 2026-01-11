using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Allows players to interact with and activate minigames when in range.
/// Automatically finds a MinigameBase component on this GameObject or its children.
/// Supports phase restrictions to limit minigame availability to specific game phases.
/// </summary>
public class MinigameActivator : MonoBehaviour
{
    [Header("Interaction")]
    /// <summary>
    /// The radius within which a player can interact with this minigame activator.
    /// </summary>
    [SerializeField] private float interactRadius = 5f;

    [Header("UI")]
    /// <summary>
    /// Optional tooltip to display when a player is in range.
    /// If null, will attempt to find one in the scene.
    /// </summary>
    [SerializeField] private InteractionTooltip interactionTooltip;
    
    /// <summary>
    /// The message to display in the tooltip.
    /// </summary>
    [SerializeField] private string tooltipMessage = "to activate minigame";
    
    /// <summary>
    /// Height offset above the minigame object where the tooltip appears.
    /// </summary>
    [SerializeField] private float tooltipHeightOffset = 1.5f;

    [Header("Phase Restriction")]
    /// <summary>
    /// Whether to enforce phase restrictions on minigame activation.
    /// </summary>
    [SerializeField] private bool enforcePhaseRestriction = true;
    
    /// <summary>
    /// The game phase during which minigames are available.
    /// </summary>
    [Tooltip("Minigames are only available during Day phase")]
    [SerializeField] private GamePhase allowedPhase = GamePhase.Day;

    /// <summary>
    /// Reference to the minigame component managed by this activator.
    /// </summary>
    private MinigameBase minigame;
    
    /// <summary>
    /// The player currently within interaction range.
    /// </summary>
    private Player _nearbyPlayer;
    
    /// <summary>
    /// Cached array of all players in the scene for performance optimization.
    /// </summary>
    private Player[] _cachedPlayers;
    
    /// <summary>
    /// Timer for refreshing the player cache.
    /// </summary>
    private float _playerCacheTimer;
    
    /// <summary>
    /// Interval in seconds between player cache refreshes.
    /// </summary>
    private const float PLAYER_CACHE_REFRESH_INTERVAL = 0.5f;
    
    /// <summary>
    /// Tracks whether the tooltip was shown in the previous frame.
    /// </summary>
    private bool _wasTooltipShown = false;

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Finds and initializes the minigame component.
    /// </summary>
    void Start()
    {
        if (minigame == null)
        {
            minigame = GetComponentInChildren<MinigameBase>(true);
            if (minigame == null)
            {
                Debug.LogWarning($"[MinigameActivator] No MinigameBase found on '{name}' or its children.");
                return;
            }
        }

        minigame.Initialize(this);
        RefreshPlayerCache();
        
        // Find or create tooltip
        if (interactionTooltip == null)
        {
            interactionTooltip = Object.FindFirstObjectByType<InteractionTooltip>();
        }
        
        if (interactionTooltip != null)
        {
            interactionTooltip.SetBaseMessage(tooltipMessage);
            interactionTooltip.Hide();
        }
    }

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Detects nearby players and handles interaction input.
    /// </summary>
    void Update()
    {
        _playerCacheTimer += Time.deltaTime;
        if (_playerCacheTimer >= PLAYER_CACHE_REFRESH_INTERVAL)
        {
            RefreshPlayerCache();
            _playerCacheTimer = 0f;
        }
        
        DetectPlayer();
        UpdateTooltip();
        
        if (_nearbyPlayer != null && WasInteractPressed())
        {
            if (CanActivate())
            {
                ToggleMinigame();
            }
            else
            {
                ShowPhaseRestrictionMessage();
            }
        }
    }

    /// <summary>
    /// Updates the tooltip visibility based on player proximity and minigame state.
    /// </summary>
    private void UpdateTooltip()
    {
        if (interactionTooltip == null) return;
        
        bool shouldShow = _nearbyPlayer != null && !minigame.IsRunning && CanActivate();
        
        if (shouldShow && !_wasTooltipShown)
        {
            // Show tooltip at position above this object
            Vector3 tooltipWorldPos = transform.position + Vector3.up * tooltipHeightOffset;
            interactionTooltip.Show(tooltipWorldPos, "Interact");
            _wasTooltipShown = true;
        }
        else if (!shouldShow && _wasTooltipShown)
        {
            interactionTooltip.Hide();
            _wasTooltipShown = false;
        }
        else if (shouldShow && _wasTooltipShown)
        {
            // Update position if tooltip is shown
            Vector3 tooltipWorldPos = transform.position + Vector3.up * tooltipHeightOffset;
            interactionTooltip.SetWorldTarget(tooltipWorldPos);
        }
    }
    
    /// <summary>
    /// Unity lifecycle method called when the component is disabled.
    /// Hides the tooltip when the activator is disabled.
    /// </summary>
    void OnDisable()
    {
        if (interactionTooltip != null && _wasTooltipShown)
        {
            interactionTooltip.Hide();
            _wasTooltipShown = false;
        }
    }

    /// <summary>
    /// Checks if the minigame can currently be activated based on phase restrictions.
    /// </summary>
    /// <returns>True if the minigame can be activated, false otherwise.</returns>
    private bool CanActivate()
    {
        if (!enforcePhaseRestriction) return true;
        
        if (GameManager.Instance == null) return true;
        
        return GameManager.Instance.CurrentPhase == allowedPhase;
    }

    /// <summary>
    /// Displays a message when the player tries to activate a minigame outside the allowed phase.
    /// </summary>
    private void ShowPhaseRestrictionMessage()
    {
        Debug.Log($"[MinigameActivator] Minigames are only available during {allowedPhase} phase!");
    }

    /// <summary>
    /// Refreshes the cached array of all players in the scene.
    /// </summary>
    private void RefreshPlayerCache()
    {
        _cachedPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// Detects the closest alive player within interaction range (excludes bots).
    /// </summary>
    private void DetectPlayer()
    {
        if (_cachedPlayers == null || _cachedPlayers.Length == 0)
        {
            _nearbyPlayer = null;
            return;
        }
        
        Player closest = null;
        float closestDist = float.MaxValue;
        var origin = transform.position;
        
        for (int i = 0; i < _cachedPlayers.Length; i++)
        {
            var p = _cachedPlayers[i];
            if (p == null || !p.IsAlive) continue;
            
            // Exclude bots - only detect actual players
            if (p is Bot) continue;
            
            float d = Vector3.Distance(p.transform.position, origin);
            if (d <= interactRadius && d < closestDist)
            {
                closestDist = d;
                closest = p;
            }
        }
        _nearbyPlayer = closest;
    }

    /// <summary>
    /// Checks if the interact key was pressed this frame.
    /// Uses KeyBindings if available, otherwise falls back to E key.
    /// </summary>
    /// <returns>True if the interact key was pressed, false otherwise.</returns>
    private bool WasInteractPressed()
    {
        var k = Keyboard.current;
        if (k == null) return false;
        
        var bindings = KeyBindings.Instance;
        if (bindings != null)
        {
            return k[bindings.Interact].wasPressedThisFrame;
        }
        
        // Fallback
        return k.eKey.wasPressedThisFrame;
    }

    /// <summary>
    /// Toggles the minigame state. Only starts if not already running.
    /// Minigames end only on timeout, not via interact key.
    /// </summary>
    private void ToggleMinigame()
    {
        if (minigame == null) return;
        // Only start the minigame if it's not running
        // Do not allow ending via interact key - minigames end only on timeout
        if (!minigame.IsRunning) 
            minigame.StartGame(_nearbyPlayer);
    }

    /// <summary>
    /// Gets whether a player is currently within interaction range.
    /// </summary>
    public bool IsPlayerInRange => _nearbyPlayer != null;
    
    /// <summary>
    /// Gets the minigame component managed by this activator.
    /// </summary>
    public MinigameBase CurrentMinigame => minigame;
    
    /// <summary>
    /// Gets whether the minigame is available in the current game phase.
    /// </summary>
    public bool IsAvailableInCurrentPhase => CanActivate();
}
