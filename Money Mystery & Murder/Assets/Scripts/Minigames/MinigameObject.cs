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
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference interactAction; // optional input action
#endif

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
        if (interactAction != null && interactAction.action != null)
        {
            return interactAction.action.WasPressedThisFrame();
        }
        var k = Keyboard.current;
        if (k != null && fallbackKey == KeyCode.E && k.eKey.wasPressedThisFrame) return true;
        return false;
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
