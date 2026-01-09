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
    }

    void Update()
    {
        DetectPlayer();
        if (_nearbyPlayer != null && WasInteractPressed())
        {
            ToggleMinigame();
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
