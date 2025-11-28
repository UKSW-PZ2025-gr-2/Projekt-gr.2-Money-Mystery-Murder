using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles player movement input and translation.
/// Supports both Unity's new Input System and legacy Input Manager.
/// Used by <see cref="Player"/> and can be disabled by <see cref="MinigameBase"/> during minigames.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Movement speed in units per second.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float moveSpeed = 5f;
    
#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// Input Action Reference for reading Vector2 move input (x,y).
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private InputActionReference moveAction;
#endif

    /// <summary>Reference to the <see cref="Player"/> component for abilities affecting speed.</summary>
    private Player _player;

    /// <summary>
    /// Initializes the <see cref="Player"/> reference.
    /// </summary>
    void Awake()
    {
        _player = GetComponent<Player>();
    }

    /// <summary>
    /// Reads movement input and translates the player each frame.
    /// </summary>
    void Update()
    {
        Vector2 move = ReadMove();
        if (move.sqrMagnitude > 1f) move.Normalize();
        transform.Translate(moveSpeed * Time.deltaTime * (Vector3)move, Space.World);
    }

    /// <summary>
    /// Reads movement input from the Input System or legacy Input Manager.
    /// </summary>
    /// <returns>A normalized Vector2 representing movement direction.</returns>
    private Vector2 ReadMove()
    {
#if ENABLE_INPUT_SYSTEM
        if (moveAction != null && moveAction.action != null)
        {
            return moveAction.action.ReadValue<Vector2>();
        }
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = 0f, y = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) y += 1f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed) y -= 1f;
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }
}
