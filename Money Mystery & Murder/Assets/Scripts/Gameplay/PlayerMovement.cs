using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement input and translation.
/// Uses Unity's new Input System.
/// Used by <see cref="Player"/> and can be disabled by <see cref="MinigameBase"/> during minigames.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Movement speed in units per second.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float moveSpeed = 5f;
    
    /// <summary>
    /// Input Action Reference for reading Vector2 move input (x,y).
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private InputActionReference moveAction;

    /// <summary>Reference to the <see cref="Player"/> component for abilities affecting speed.</summary>
    private Player _player;
    
    /// <summary>Reference to the <see cref="PlayerAnimator"/> component for controlling animations.</summary>
    private PlayerAnimator _playerAnimator;
    
    /// <summary>Reference to the sprite renderer for flipping the sprite.</summary>
    private SpriteRenderer _spriteRenderer;
    
    /// <summary>Current speed multiplier applied to movement (default 1.0).</summary>
    private float _speedMultiplier = 1f;
    
    /// <summary>Footstep sound timing</summary>
    private float _footstepTimer = 0f;
    private float _footstepInterval = 0.5f;

    /// <summary>
    /// Initializes the <see cref="Player"/> reference.
    /// </summary>
    void Awake()
    {
        _player = GetComponent<Player>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("[PlayerMovement] No SpriteRenderer found on player or children. Sprite flipping will not work.");
        }
    }

    /// <summary>
    /// Reads movement input and translates the player each frame.
    /// </summary>
    void Update()
    {
        Vector2 move = ReadMove();
        if (move.sqrMagnitude > 1f) move.Normalize();
        
        bool isMoving = move.sqrMagnitude > 0.01f;
        
        if (_playerAnimator != null)
        {
            _playerAnimator.SetMovementState(isMoving);
        }
        
        if (isMoving)
        {
            transform.Translate(moveSpeed * _speedMultiplier * Time.deltaTime * (Vector3)move, Space.World);
            
            // Flip the sprite based on horizontal movement
            if (Mathf.Abs(move.x) > 0.01f)
            {
                FlipSprite(move.x);
            }
            
            // Play footstep sounds
            _footstepTimer += Time.deltaTime;
            if (_footstepTimer >= _footstepInterval)
            {
                _footstepTimer = 0f;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayFootstep();
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    /// <summary>
    /// Flips the sprite based on horizontal movement direction.
    /// </summary>
    /// <param name="horizontalInput">Horizontal movement input (-1 for left, +1 for right).</param>
    private void FlipSprite(float horizontalInput)
    {
        if (_spriteRenderer == null) return;
        
        if (horizontalInput > 0)
        {
            // Moving right, face right (not flipped)
            _spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            // Moving left, face left (flipped)
            _spriteRenderer.flipX = true;
        }
    }

    /// <summary>
    /// Sets the speed multiplier for movement effects (e.g., speed boost ability).
    /// </summary>
    /// <param name="multiplier">Speed multiplier (1.0 = normal speed, 2.0 = double speed, etc.)</param>
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// Reads movement input from the new Input System.
    /// </summary>
    /// <returns>A normalized Vector2 representing movement direction.</returns>
    private Vector2 ReadMove()
    {
        if (moveAction != null && moveAction.action != null)
        {
            return moveAction.action.ReadValue<Vector2>();
        }
        
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        
        var bindings = KeyBindings.Instance;
        if (bindings == null) return Vector2.zero;
        
        float x = 0f, y = 0f;
        
        if (k[bindings.MoveLeft].isPressed) x -= 1f;
        if (k[bindings.MoveRight].isPressed) x += 1f;
        if (k[bindings.MoveUp].isPressed) y += 1f;
        if (k[bindings.MoveDown].isPressed) y -= 1f;
        
        return new Vector2(x, y);
    }
}
