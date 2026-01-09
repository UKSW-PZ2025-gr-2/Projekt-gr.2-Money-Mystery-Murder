using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to an object to allow a player to interact and open a shop.
/// Automatically finds a ShopUI component on this GameObject or its children.
/// </summary>
public class ShopActivator : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 5f;
    [SerializeField] private Key interactKey = Key.E;

    private ShopUI shopUI;
    private Player _nearbyPlayer;
    private Player[] _cachedPlayers;
    private float _playerCacheTimer;
    private const float PLAYER_CACHE_REFRESH_INTERVAL = 0.5f;

    void Start()
    {
        if (shopUI == null)
        {
            shopUI = GetComponentInChildren<ShopUI>(true);
            if (shopUI == null)
            {
                Debug.LogWarning($"[ShopActivator] No ShopUI found on '{name}' or its children.");
                return;
            }
        }

        shopUI.Initialize(this);
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
            ToggleShop();
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
        
        return k[interactKey].wasPressedThisFrame;
    }

    private void ToggleShop()
    {
        if (shopUI == null) return;
        if (shopUI.IsOpen) 
            shopUI.CloseShop(); 
        else 
            shopUI.OpenShop(_nearbyPlayer);
    }

    public bool IsPlayerInRange => _nearbyPlayer != null;
    public ShopUI CurrentShop => shopUI;
}
