using UnityEngine;

/// <summary>
/// Grenade projectile that explodes after a fuse timer, dealing area damage to nearby players.
/// Does not explode on impact, allowing the grenade to bounce before detonation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GrenadeProjectile : MonoBehaviour
{
    /// <summary>
    /// The amount of damage dealt by the explosion.
    /// </summary>
    public int damage = 70;
    
    /// <summary>
    /// The explosion radius in world units.
    /// </summary>
    public float radius = 2.5f;
    
    /// <summary>
    /// Fuse time in seconds before the grenade explodes.
    /// </summary>
    public float fuse = 2.0f;
    
    /// <summary>
    /// The GameObject that threw this grenade (prevents self-damage).
    /// </summary>
    public GameObject owner;
    
    /// <summary>
    /// The Player component of the owner, used for tracking kills.
    /// </summary>
    public Player ownerPlayer;
    
    /// <summary>
    /// The visual effect prefab spawned on explosion.
    /// </summary>
    public GameObject explosionEffectPrefab;
    
    [Header("Optimization")]
    /// <summary>
    /// Layer mask for targeting players. Set to 'Player' layer for optimal performance.
    /// </summary>
    [Tooltip("Set to 'Player' layer for optimal performance. Leave at 0 to hit everything.")]
    public LayerMask targetLayers;

    /// <summary>
    /// Tracks whether the grenade has already exploded to prevent multiple explosions.
    /// </summary>
    private bool hasExploded = false;
    
    /// <summary>
    /// Cached array for explosion overlap detection to reduce allocations.
    /// </summary>
    private static Collider2D[] explosionHits = new Collider2D[20];

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Schedules the explosion after the fuse duration.
    /// </summary>
    private void Start()
    {
        Invoke(nameof(Explode), fuse);
    }

    /// <summary>
    /// Unity physics callback invoked when this grenade collides with another collider.
    /// Grenades do not explode on impact, allowing them to bounce.
    /// </summary>
    /// <param name="collision">The collision data.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't explode on impact - only after fuse timer
        // This allows the grenade to bounce and fly
    }

    /// <summary>
    /// Triggers the grenade explosion, spawning visual effects and dealing area damage.
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }

        // Optimized area damage with layer filtering and cached array
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, explosionHits, targetLayers != 0 ? targetLayers : ~0);
        
        for (int i = 0; i < hitCount; i++)
        {
            if (explosionHits[i] == null) continue;
            
            // Optimized: Try direct component first
            var p = explosionHits[i].GetComponent<Player>();
            if (p == null)
            {
                p = explosionHits[i].GetComponentInParent<Player>();
            }
            
            if (p != null && (owner == null || p.gameObject != owner))
            {
                p.TakeDamage(damage, ownerPlayer);
            }
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Unity editor callback that draws gizmos when the object is selected.
    /// Visualizes the explosion radius.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
