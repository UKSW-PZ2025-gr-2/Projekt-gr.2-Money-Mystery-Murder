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
    public GameObject explosionEffectPrefab;

    private bool hasExploded = false;

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

        // Deal area damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var c in hits)
        {
            var p = c.GetComponentInParent<Player>() ?? c.GetComponent<Player>();
            if (p != null && (owner == null || p.gameObject != owner))
            {
                p.TakeDamage(damage);
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
