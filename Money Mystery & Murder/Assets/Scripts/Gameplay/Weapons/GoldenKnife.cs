using UnityEngine;

/// <summary>
/// Rare powerful variant of the <see cref="Knife"/> weapon with significantly enhanced stats.
/// Awarded through special events like the slot machine jackpot.
/// </summary>
public class GoldenKnife : MeleeWeapon
{
    [Header("Golden Knife Stats Overrides")]
    [SerializeField] private int overrideDamage = 75;
    [SerializeField] private float overrideCooldown = 0.25f;
    [SerializeField] private float overrideRange = 1.5f;
    [Header("Golden Knife Visuals")]
    [SerializeField] private GameObject hitEffectPrefab;
    private Animator animator;
    [Header("Hit Detection")]
    [Tooltip("Layers that can be damaged by this melee weapon (set to Player layer for player-only hits)")]
    [SerializeField] private LayerMask hitLayers = ~0;

    private void Awake()
    {
        // Apply configured overrides to the base weapon stats
        damage = overrideDamage;
        cooldown = overrideCooldown;
        range = overrideRange;

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

        // Determine spawn position for the visual effect
        Vector3 spawnPos = transform.position;
        Vector3 forward = transform.forward;

        // Spawn hit effect if assigned
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.LookRotation(forward));
            Destroy(effect, 2f);
        }

        // Use multiple raycasts in a spread pattern to detect nearby players reliably
        var damaged = new System.Collections.Generic.HashSet<Player>();
        RaycastHit2D hit;

        // Cast a wider spread of raycasts for the golden knife (more coverage)
        Vector3[] directions = new Vector3[]
        {
            forward,
            forward + transform.right * 0.4f,
            forward - transform.right * 0.4f,
            forward + transform.up * 0.3f,
            forward - transform.up * 0.3f,
            forward + transform.right * 0.2f + transform.up * 0.2f,
            forward - transform.right * 0.2f - transform.up * 0.2f
        };

        Debug.Log($"GoldenKnife.Attack(): startPos={spawnPos:F2} forward={forward:F2} casting {directions.Length} Physics2D raycasts with range {range}");

        foreach (Vector3 dir in directions)
        {
            Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
            Debug.DrawRay(spawnPos, dir * range, Color.yellow, 0.5f);
            
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
                Debug.Log($"GoldenKnife hit player {p.name} for {damage} damage. New HP={p.CurrentHealth}");
            }
            else
            {
                Debug.Log($"  Raycast missed: {dir2D}");
            }
        }

        if (damaged.Count == 0)
        {
            Debug.Log("GoldenKnife.Attack(): no players hit by raycasts");
        }
        
        Debug.Log($"GoldenKnife.Attack() - Damage={damage}, Cooldown={cooldown}, Range={range}");
    }
}
