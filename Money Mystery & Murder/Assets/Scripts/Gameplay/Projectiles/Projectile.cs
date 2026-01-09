using UnityEngine;

/// <summary>
/// Simple 2D projectile that moves by Rigidbody2D velocity and deals damage on collision.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject owner;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Debug.Log($"[Projectile] Awake: {gameObject.name} rb={_rb != null}");
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
        Debug.Log($"[Projectile] Start: {gameObject.name} damage={damage} speed={speed} owner={owner?.name ?? "null"}");
    }

    public void Launch(Vector2 direction)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = direction.normalized * speed;
        Debug.Log($"[Projectile] Launch: direction={direction} velocity={_rb.linearVelocity} speed={speed}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Projectile] OnCollisionEnter2D: hit {collision.collider.name}");
        
        if (collision == null || collision.collider == null) return;

        var player = collision.collider.GetComponentInParent<Player>() ?? collision.collider.GetComponent<Player>();
        if (player != null && (owner == null || player.gameObject != owner))
        {
            Debug.Log($"[Projectile] Damaging player {player.name} for {damage} damage");
            player.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
