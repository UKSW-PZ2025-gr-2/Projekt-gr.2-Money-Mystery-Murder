using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
#if ENABLE_INPUT_SYSTEM
    [Header("Input System")]
    [Tooltip("Reference to a Vector2 movement action (WASD / Arrow keys / Left Stick). Should deliver X,Y where Y is vertical movement.")]
    [SerializeField] private InputActionReference moveAction; // expected Vector2
#endif

#if ENABLE_INPUT_SYSTEM
    void OnEnable()
    {
        if (moveAction != null && moveAction.action != null && !moveAction.action.enabled)
        {
            moveAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
        {
            moveAction.action.Disable();
        }
    }
#endif

    void Update()
    {
        Vector3 moveDir = ReadMovementInput(); // X,Y plane
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        // No rotation for 2D movement.
    }

    private Vector3 ReadMovementInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (moveAction != null && moveAction.action != null)
        {
            Vector2 v2 = moveAction.action.ReadValue<Vector2>();
            return new Vector3(v2.x, v2.y, 0f);
        }
        if (Keyboard.current != null)
        {
            float h = 0f; float y = 0f;
            var k = Keyboard.current;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) h -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) h += 1f;
            if (k.wKey.isPressed || k.upArrowKey.isPressed) y += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) y -= 1f;
            return new Vector3(h, y, 0f);
        }
        return Vector3.zero;
#else
        float h = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        return new Vector3(h, y, 0f);
#endif
    }
}
