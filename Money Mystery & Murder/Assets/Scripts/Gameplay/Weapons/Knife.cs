using UnityEngine;

public class Knife : MeleeWeapon
{
    [Header("Knife Stats Overrides")]
    [SerializeField] private int overrideDamage = 20;
    [SerializeField] private float overrideCooldown = 0.5f;
    [SerializeField] private float overrideRange = 0.75f;
    [Header("Knife Visuals")]
    [SerializeField] private GameObject hitEffectPrefab;
    private Animator animator;
    [Header("Hit Detection")]
    [Tooltip("Layers that can be damaged by this melee weapon (set to Player layer for player-only hits)")]
    [SerializeField] private LayerMask hitLayers = ~0;

    private void Awake()
    {
        // Apply configured overrides to the base weapon stats
        // `damage`, `cooldown` and `range` are protected in WeaponController
        damage = overrideDamage;
        cooldown = overrideCooldown;
        range = overrideRange;

        // Try to get an Animator on this GameObject for playing stab animations.
        animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        // Respect cooldown
        if (Time.time - lastAttackTime < cooldown)
            return;

        lastAttackTime = Time.time;

        // Trigger stab animation if animator is available
        if (animator != null)
        {
            animator.SetTrigger("Stab");
        }

        // Determine spawn position for the visual effect. Use the weapon's world transform
        // (so detection matches where the knife is visible). Owner is kept for ignore checks only.
        Vector3 spawnPos = transform.position;
        Vector3 forward = transform.forward;

        // Spawn hit effect if assigned
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.LookRotation(forward));
            Destroy(effect, 2f);
        }

        // Use multiple raycasts in a spread pattern to detect nearby players reliably.
        // Since players use 2D colliders (BoxCollider2D), we use Physics2D raycasts.
        var damaged = new System.Collections.Generic.HashSet<Player>();
        RaycastHit2D hit;

        // Cast a spread of raycasts in cone-like pattern forward and slightly to sides
        Vector3[] directions = new Vector3[]
        {
            forward,
            forward + transform.right * 0.3f,
            forward - transform.right * 0.3f,
            forward + transform.up * 0.2f,
            forward - transform.up * 0.2f
        };

        Debug.Log($"Knife.Attack(): startPos={spawnPos:F2} forward={forward:F2} casting {directions.Length} Physics2D raycasts with range {range}");

        foreach (Vector3 dir in directions)
        {
            Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
            Debug.DrawRay(spawnPos, dir * range, Color.red, 0.5f);
            
            hit = Physics2D.Raycast(new Vector2(spawnPos.x, spawnPos.y), dir2D, range);
            
            if (hit.collider != null)
            {
                Debug.Log($"  Raycast HIT: {hit.collider.name} at distance {hit.distance:F2}");
                // Try to find Player component on the hit object
                Player p = hit.collider.GetComponent<Player>();
                if (p == null) p = hit.collider.GetComponentInParent<Player>();
                if (p == null) p = hit.collider.GetComponentInChildren<Player>();

                // Skip owner
                if (p == owner) 
                {
                    Debug.Log($"    Skipping owner: {p.name}");
                    continue;
                }
                if (p == null) 
                {
                    Debug.Log($"    No Player component found on {hit.collider.name}");
                    continue;
                }

                // Avoid double-damaging same player in one attack
                if (damaged.Contains(p)) continue;
                damaged.Add(p);

                p.TakeDamage(damage);
                Debug.Log($"Knife hit player {p.name} for {damage} damage. New HP={p.CurrentHP}");
            }
            else
            {
                Debug.Log($"  Raycast missed: {dir2D}");
            }
        }

        if (damaged.Count == 0)
        {
            Debug.Log("Knife.Attack(): no players hit by raycasts");
        }        // Optional: play a quick debug log (can be removed later)
        Debug.Log($"Knife.Attack() - Damage={damage}, Cooldown={cooldown}, Range={range}");
    }
}
