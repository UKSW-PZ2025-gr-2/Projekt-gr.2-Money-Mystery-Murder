using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Attach to an object to allow a player to interact and open a minigame.
// Automatically finds a MinigameBase component on this GameObject or its children.
public class MinigameObject : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2f;
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
        var player = _nearbyPlayer ?? Object.FindFirstObjectByType<Player>();
        if (player != null && Vector3.Distance(player.transform.position, transform.position) <= interactRadius)
        {
            _nearbyPlayer = player;
        }
        else
        {
            _nearbyPlayer = null;
        }
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
