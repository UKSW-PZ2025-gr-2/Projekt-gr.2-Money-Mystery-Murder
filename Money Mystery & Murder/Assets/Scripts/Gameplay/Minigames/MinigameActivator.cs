using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Attach to an object to allow a player to interact and open a minigame.
// Automatically finds a MinigameBase component on this GameObject or its children.
public class MinigameActivator : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 5f;
    private KeyCode fallbackKey = KeyCode.E; // legacy input interact key (hidden from inspector)

    private MinigameBase minigame; // auto-found minigame component (UI / logic)

    private Player _nearbyPlayer;

    void Start()
    {
        if (minigame == null)
        {
            // Prefer local/child component to avoid scene-wide references
            minigame = GetComponentInChildren<MinigameBase>(true);
            if (minigame == null)
            {
                Debug.LogWarning($"[MinigameObject] No MinigameBase found on '{name}' or its children.");
                return;
            }
        }

        minigame.Initialize(this); // host for callbacks
        // No automatic enabling/disabling of objects here
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
        // Pick the closest player within interact radius. This better matches
        // the intent of the player who pressed the keyboard.
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
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return false;
        // Support a few common keys; extend as needed
        return fallbackKey switch
        {
            KeyCode.E => k.eKey.wasPressedThisFrame,
            KeyCode.F => k.fKey.wasPressedThisFrame,
            KeyCode.Space => k.spaceKey.wasPressedThisFrame,
            KeyCode.Return => k.enterKey.wasPressedThisFrame,
            KeyCode.Escape => k.escapeKey.wasPressedThisFrame,
            _ => false
        };
#else
        return Input.GetKeyDown(fallbackKey);
#endif
    }

    private void ToggleMinigame()
    {
        if (minigame == null) return;
        if (minigame.IsRunning) minigame.EndGame(); else minigame.StartGame(_nearbyPlayer);
    }

    // Public API
    public bool IsPlayerInRange => _nearbyPlayer != null;
    public MinigameBase CurrentMinigame => minigame;
}
