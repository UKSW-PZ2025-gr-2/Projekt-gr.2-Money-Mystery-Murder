using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Attach to an object to allow a player to interact and open a minigame.
// Requires only a reference to a MinigameBase component assigned in the inspector.
public class MinigameObject : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private KeyCode fallbackKey = KeyCode.E; // legacy input interact key

    [Header("Minigame Reference")]
    [SerializeField] private MinigameBase minigame; // existing minigame component (UI / logic)

    private Player _nearbyPlayer;

    void Start()
    {
        if (minigame != null)
        {
            minigame.Initialize(this); // host for callbacks
            minigame.gameObject.SetActive(false); // hidden until started
        }
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
