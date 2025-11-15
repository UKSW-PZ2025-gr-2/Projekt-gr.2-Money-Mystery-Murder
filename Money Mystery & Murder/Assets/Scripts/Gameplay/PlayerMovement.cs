using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference moveAction; // Vector2 (x,y)
#endif

    private Player _player; // for abilities affecting speed

    void Awake()
    {
        _player = GetComponent<Player>();
    }

    void Update()
    {
        Vector2 move = ReadMove();
        if (move.sqrMagnitude > 1f) move.Normalize();
        float speedMult = _player != null ? _player.GetSpeedMultiplier() : 1f;
        transform.Translate((Vector3)move * moveSpeed * speedMult * Time.deltaTime, Space.World);
    }

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
