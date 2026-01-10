using UnityEngine;
using UnityEngine.InputSystem;

// Attach to an object to allow a player to interact and open a minigame.
// Automatically finds a MinigameBase component on this GameObject or its children.
public class MinigameActivator : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 5f;

    [Header("Phase Restriction")]
    [SerializeField] private bool enforcePhaseRestriction = true;
    [Tooltip("Minigames are only available during Day phase")]
    [SerializeField] private GamePhase allowedPhase = GamePhase.Day;

    private MinigameBase minigame;
    private Player _nearbyPlayer;
    private Player[] _cachedPlayers;
    private float _playerCacheTimer;
    private const float PLAYER_CACHE_REFRESH_INTERVAL = 0.5f;

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
    }

    void Update()
    {
        _playerCacheTimer += Time.deltaTime;
        if (_playerCacheTimer >= PLAYER_CACHE_REFRESH_INTERVAL)
        {
            RefreshPlayerCache();
            _playerCacheTimer = 0f;
        }
        
        DetectPlayer();
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

    private bool CanActivate()
    {
        if (!enforcePhaseRestriction) return true;
        
        if (GameManager.Instance == null) return true;
        
        return GameManager.Instance.CurrentPhase == allowedPhase;
    }

    private void ShowPhaseRestrictionMessage()
    {
        Debug.Log($"[MinigameActivator] Minigames are only available during {allowedPhase} phase!");
    }

    private void RefreshPlayerCache()
    {
        _cachedPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
    }

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

    private void ToggleMinigame()
    {
        if (minigame == null) return;
        // Only start the minigame if it's not running
        // Do not allow ending via interact key - minigames end only on timeout
        if (!minigame.IsRunning) 
            minigame.StartGame(_nearbyPlayer);
    }

    public bool IsPlayerInRange => _nearbyPlayer != null;
    public MinigameBase CurrentMinigame => minigame;
    public bool IsAvailableInCurrentPhase => CanActivate();
}
