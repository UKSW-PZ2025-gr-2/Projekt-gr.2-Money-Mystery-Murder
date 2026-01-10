using UnityEngine;

/// <summary>
/// Grenade projectile: explodes after fuse or on impact, dealing area damage.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GrenadeProjectile : MonoBehaviour
{
    public int damage = 70;
    public float radius = 2.5f;
    public float fuse = 2.0f;
    public GameObject owner;
    public Player ownerPlayer;
    public GameObject explosionEffectPrefab;
    
    [Header("Optimization")]
    [Tooltip("Set to 'Player' layer for optimal performance. Leave at 0 to hit everything.")]
    public LayerMask targetLayers;

    private bool hasExploded = false;
    
    // Cache for performance
    private static Collider2D[] explosionHits = new Collider2D[20];

    private void Start()
    {
        Invoke(nameof(Explode), fuse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't explode on impact - only after fuse timer
        // This allows the grenade to bounce and fly
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
