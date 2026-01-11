using UnityEngine;

/// <summary>
/// Simple 2D projectile that moves by Rigidbody2D velocity and deals damage on collision.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// The amount of damage this projectile deals on impact.
    /// </summary>
    public int damage = 10;
    
    /// <summary>
    /// The movement speed of the projectile in units per second.
    /// </summary>
    public float speed = 20f;
    
    /// <summary>
    /// The lifetime of the projectile in seconds before it is destroyed.
    /// </summary>
    public float lifetime = 5f;
    
    /// <summary>
    /// The GameObject that fired this projectile (prevents self-collision).
    /// </summary>
    public GameObject owner;
    
    /// <summary>
    /// The Player component of the owner, used for tracking kills.
    /// </summary>
    public Player ownerPlayer;

    /// <summary>
    /// Cached Rigidbody2D component used for movement.
    /// </summary>
    private Rigidbody2D _rb;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Initializes the Rigidbody2D component and configures physics settings.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        // Changed from Continuous to Discrete for better performance
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        Debug.Log($"[Projectile] Awake: {gameObject.name} rb={_rb != null}");
    }

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Schedules the projectile for destruction after its lifetime expires.
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, lifetime);
        Debug.Log($"[Projectile] Start: {gameObject.name} damage={damage} speed={speed} owner={owner?.name ?? "null"}");
    }

    /// <summary>
    /// Launches the projectile in the specified direction.
    /// </summary>
    /// <param name="direction">The direction vector to launch the projectile.</param>
    public void Launch(Vector2 direction)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = direction.normalized * speed;
        Debug.Log($"[Projectile] Launch: direction={direction} velocity={_rb.linearVelocity} speed={speed}");
    }

    /// <summary>
    /// Unity physics callback invoked when this projectile collides with another collider.
    /// Deals damage to players and destroys the projectile.
    /// </summary>
    /// <param name="collision">The collision data containing information about the collision.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Projectile] OnCollisionEnter2D: hit {collision.collider.name}");
        
        if (collision == null || collision.collider == null) return;

        // Optimized: Try direct component first
        var player = collision.collider.GetComponent<Player>();
        if (player == null)
        {
            player = collision.collider.GetComponentInParent<Player>();
        }
        
        if (player != null && (owner == null || player.gameObject != owner))
        {
            Debug.Log($"[Projectile] Damaging player {player.name} for {damage} damage");
            player.TakeDamage(damage, ownerPlayer);
        }

        Destroy(gameObject);
    }
}
