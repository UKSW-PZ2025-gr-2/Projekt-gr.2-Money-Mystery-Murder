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
    }

    void Update()
    {
        DetectPlayer();
        if (_nearbyPlayer != null && WasInteractPressed())
        {
            ToggleShop();
        }
    }

    private void DetectPlayer()
    {
        var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        Player closest = null;
        float closestDist = float.MaxValue;
        var origin = transform.position;
        
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
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
