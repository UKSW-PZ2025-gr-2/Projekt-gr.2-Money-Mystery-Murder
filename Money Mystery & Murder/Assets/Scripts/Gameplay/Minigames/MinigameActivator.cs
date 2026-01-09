using UnityEngine;
using UnityEngine.InputSystem;

// Attach to an object to allow a player to interact and open a minigame.
// Automatically finds a MinigameBase component on this GameObject or its children.
public class MinigameActivator : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 5f;

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
            ToggleMinigame();
        }
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
        if (minigame.IsRunning) 
            minigame.EndGame(); 
        else 
            minigame.StartGame(_nearbyPlayer);
    }

    public bool IsPlayerInRange => _nearbyPlayer != null;
    public MinigameBase CurrentMinigame => minigame;
}
