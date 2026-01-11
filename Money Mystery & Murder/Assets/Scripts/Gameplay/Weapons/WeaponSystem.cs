using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unified weapon system that handles all weapon logic based on WeaponData.
/// Replaces the old WeaponController, MeleeWeapon, Knife, and GoldenKnife classes.
/// Supports both melee and ranged weapons with hit detection, cooldowns, and phase restrictions.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Current Weapon")]
    /// <summary>
    /// The currently equipped weapon data.
    /// </summary>
    [SerializeField] private WeaponData currentWeapon;
    
    [Header("Hit Detection")]
    /// <summary>
    /// Layer mask for hit detection. Set to 'Player' layer for optimal performance.
    /// </summary>
    [SerializeField, Tooltip("Set to 'Player' layer for optimal performance")]
    private LayerMask hitLayers;
    
    [Header("Visual References")]
    /// <summary>
    /// Animator component for weapon animations.
    /// </summary>
    [SerializeField] private Animator weaponAnimator;

    [Header("Phase Restriction")]
    /// <summary>
    /// Whether to enforce phase restrictions on attacking (e.g., only allow attacks during Night phase).
    /// </summary>
    [SerializeField] private bool enforcePhaseRestriction = true;
    
    /// <summary>
    /// The game phase during which attacking is allowed.
    /// </summary>
    [Tooltip("Attacking is only available during Night phase")]
    [SerializeField] private GamePhase allowedPhase = GamePhase.Night;
    
    /// <summary>
    /// The player who owns this weapon system.
    /// </summary>
    private Player owner;
    
    /// <summary>
    /// The time of the last attack, used for cooldown calculations.
    /// </summary>
    private float lastAttackTime;
    
    /// <summary>
    /// Current ammunition count for weapons that use ammo.
    /// </summary>
    private int currentAmmo;
    
    /// <summary>
    /// The instantiated weapon visual GameObject.
    /// </summary>
    private GameObject weaponVisual;
    
    /// <summary>
    /// The time of the last phase restriction warning to prevent spam.
    /// </summary>
    private float lastPhaseWarningTime;
    
    /// <summary>
    /// Cooldown duration between phase restriction warnings.
    /// </summary>
    private const float PHASE_WARNING_COOLDOWN = 2f;
    
    /// <summary>
    /// Cached array for raycast hit detection to reduce allocations.
    /// </summary>
    private static RaycastHit2D[] raycastHits = new RaycastHit2D[10];
    
    /// <summary>
    /// Gets the currently equipped weapon data.
    /// </summary>
    public WeaponData CurrentWeapon => currentWeapon;
    
    /// <summary>
    /// Gets the current ammunition count.
    /// </summary>
    public int CurrentAmmo => currentAmmo;
    
    /// <summary>
    /// Initialize the weapon system with the owning player.
    /// </summary>
    /// <param name="player">The player who owns this weapon system.</param>
    public void Initialize(Player player)
    {
        owner = player;
        lastAttackTime = Time.time - (currentWeapon?.cooldown ?? 0f);
    }
    
    /// <summary>
    /// Equip a new weapon from weapon data.
    /// </summary>
    /// <param name="weapon">The weapon data to equip.</param>
    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        
        currentWeapon = weapon;
        currentAmmo = weapon.usesAmmo ? weapon.maxAmmo : 0;
        lastAttackTime = Time.time - weapon.cooldown;
        
        UpdateWeaponVisual();
    }
    
    /// <summary>
    /// Attempt to perform an attack with the current weapon.
    /// Checks cooldown, ammunition, and phase restrictions before attacking.
    /// </summary>
    public void Attack()
    {
        if (currentWeapon == null) return;
        if (!CanAttack()) 
        {
            if (!IsInAllowedPhase())
            {
                ShowPhaseRestrictionWarning();
            }
            return;
        }
        
        lastAttackTime = Time.time;
        
        if (currentWeapon.usesAmmo)
        {
            currentAmmo--;
        }
        
        PlayAttackAnimation();
        PlayAttackSound();
        
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Melee:
                PerformMeleeAttack();
                break;
            case WeaponType.Ranged:
                PerformRangedAttack();
                break;
        }
    }
    
    /// <summary>
    /// Check if the weapon can currently attack.
    /// Considers cooldown, ammunition, phase restrictions, and player state.
    /// </summary>
    /// <returns>True if the weapon can attack, false otherwise.</returns>
    public bool CanAttack()
    {
        if (currentWeapon == null) return false;
        if (owner != null && owner.IsInMinigameOrShop()) return false;
        if (!IsInAllowedPhase()) return false;
        if (Time.time - lastAttackTime < currentWeapon.cooldown) return false;
        if (currentWeapon.usesAmmo && currentAmmo <= 0) return false;
        return true;
    }

    /// <summary>
    /// Check if current phase allows attacking based on phase restriction settings.
    /// </summary>
    /// <returns>True if attacking is allowed in the current phase.</returns>
    private bool IsInAllowedPhase()
    {
        if (!enforcePhaseRestriction) return true;
        
        if (GameManager.Instance == null) return true;
        
        return GameManager.Instance.CurrentPhase == allowedPhase;
    }

    /// <summary>
    /// Show a warning message when trying to attack outside the allowed phase.
    /// Cooldown prevents spam.
    /// </summary>
    private void ShowPhaseRestrictionWarning()
    {
        if (Time.time - lastPhaseWarningTime < PHASE_WARNING_COOLDOWN) return;
        
        lastPhaseWarningTime = Time.time;
        Debug.Log($"[WeaponSystem] Attacking is only available during {allowedPhase} phase!");
    }
    
    /// <summary>
    /// Get the time remaining until the next attack is available.
    /// </summary>
    /// <returns>Remaining cooldown time in seconds.</returns>
    public float GetCooldownRemaining()
    {
        if (currentWeapon == null) return 0f;
        return Mathf.Max(0f, currentWeapon.cooldown - (Time.time - lastAttackTime));
    }
    
    /// <summary>
    /// Performs a melee attack using arc-based raycast hit detection.
    /// </summary>
    private void PerformMeleeAttack()
    {
        Vector3 origin = GetAttackOrigin();
        Vector3 forward = transform.right;
        
        SpawnHitEffect(origin, forward);
        
        var damaged = new HashSet<Player>();
        Vector3[] directions = GenerateMeleeRayDirections(forward);

        foreach (Vector3 dir in directions)
        {
            Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
            Debug.DrawRay(origin, dir * currentWeapon.range, Color.red, 0.5f);

            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(origin.x, origin.y),
                dir2D,
                currentWeapon.range,
                hitLayers
            );

            if (hit.collider != null)
            {
                Player target = GetPlayerFromCollider(hit.collider);

                if (target != null && target != owner && !damaged.Contains(target))
                {
                    damaged.Add(target);
                    target.TakeDamage(currentWeapon.damage, owner);
                }
            }
        }
    }
    
    /// <summary>
    /// Performs a ranged attack by spawning projectiles or using raycast hit detection.
    /// </summary>
    private void PerformRangedAttack()
    {
        Vector3 origin = GetAttackOrigin();
        Vector3 direction = transform.right;
        
        if (currentWeapon.projectilePrefab != null)
        {
            GameObject projectile = Instantiate(
                currentWeapon.projectilePrefab, 
                origin, 
                Quaternion.identity
            );
            
            // Set owner for grenades
            var grenadeProjectile = projectile.GetComponent<GrenadeProjectile>();
            if (grenadeProjectile != null)
            {
                grenadeProjectile.owner = owner?.gameObject;
                grenadeProjectile.ownerPlayer = owner;
                grenadeProjectile.explosionEffectPrefab = currentWeapon.hitEffectPrefab;
            }
            
            // Set owner for regular projectiles (bullets)
            var bulletProjectile = projectile.GetComponent<Projectile>();
            if (bulletProjectile != null)
            {
                bulletProjectile.owner = owner?.gameObject;
                bulletProjectile.ownerPlayer = owner;
                bulletProjectile.damage = currentWeapon.damage;
                bulletProjectile.Launch(direction);
            }
            
            // Fallback: Try 2D rigidbody first (for grenades), then 3D
            if (grenadeProjectile != null)
            {
                Rigidbody2D rb2d = projectile.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.linearVelocity = direction * currentWeapon.projectileSpeed;
                }
                else
                {
                    Rigidbody rb = projectile.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = direction * currentWeapon.projectileSpeed;
                    }
                }
            }
        }
        else
        {
            Vector2 dir2D = new Vector2(direction.x, direction.y).normalized;

            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(origin.x, origin.y),
                dir2D,
                currentWeapon.range,
                hitLayers
            );

            if (hit.collider != null)
            {
                Player target = GetPlayerFromCollider(hit.collider);

                if (target != null && target != owner)
                {
                    target.TakeDamage(currentWeapon.damage, owner);
                }
            }
        }
    }
    
    /// <summary>
    /// Generates multiple ray directions for melee attack spread based on weapon settings.
    /// </summary>
    /// <param name="forward">The forward direction of the attack.</param>
    /// <returns>Array of ray directions within the attack arc.</returns>
    private Vector3[] GenerateMeleeRayDirections(Vector3 forward)
    {
        int spreadCount = Mathf.Max(1, currentWeapon.raycastSpread);
        Vector3[] directions = new Vector3[spreadCount];
        
        if (spreadCount == 1)
        {
            directions[0] = forward;
            return directions;
        }
        
        float angleStep = currentWeapon.hitArcDegrees / (spreadCount - 1);
        float startAngle = -currentWeapon.hitArcDegrees / 2f;
        
        for (int i = 0; i < spreadCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            
            float offsetX = Mathf.Sin(angle * Mathf.Deg2Rad) * 0.4f;
            float offsetY = Mathf.Cos(angle * Mathf.Deg2Rad) * 0.2f;
            
            directions[i] = (forward + transform.right * offsetX + transform.up * offsetY).normalized;
        }
        
        return directions;
    }
    
    /// <summary>
    /// Gets the origin point for attack raycasts or projectile spawning.
    /// </summary>
    /// <returns>The world position where attacks originate.</returns>
    private Vector3 GetAttackOrigin()
    {
        return transform.position;
    }
    
    /// <summary>
    /// Attempts to get a Player component from a collider with optimized search order.
    /// </summary>
    /// <param name="collider">The collider to search.</param>
    /// <returns>The Player component if found, null otherwise.</returns>
    private Player GetPlayerFromCollider(Collider2D collider)
    {
        // Optimized: Try direct component first (most common case)
        Player player = collider.GetComponent<Player>();
        if (player != null) return player;
        
        // Fallback to parent search (less common)
        player = collider.GetComponentInParent<Player>();
        if (player != null) return player;
        
        // Last resort: check children (rare)
        return collider.GetComponentInChildren<Player>();
    }
    
    /// <summary>
    /// Spawns a visual hit effect at the attack position.
    /// </summary>
    /// <param name="position">World position to spawn the effect.</param>
    /// <param name="forward">Forward direction for effect orientation.</param>
    private void SpawnHitEffect(Vector3 position, Vector3 forward)
    {
        if (currentWeapon.hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                currentWeapon.hitEffectPrefab, 
                position, 
                Quaternion.LookRotation(forward)
            );
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// Plays the weapon attack animation if an animator is assigned.
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
    }
    
    /// <summary>
    /// Plays the weapon attack sound effect based on weapon type.
    /// </summary>
    private void PlayAttackSound()
    {
        // Play weapon-specific sound
        if (currentWeapon != null && AudioManager.Instance != null)
        {
            if (currentWeapon.weaponType == WeaponType.Melee)
            {
                AudioManager.Instance.PlayKnife();
            }
            else if (currentWeapon.weaponType == WeaponType.Ranged)
            {
                AudioManager.Instance.PlayRifleShoot();
            }
        }
        
        // Also play weapon's own sound if it has one
        if (currentWeapon.attackSound != null && AudioManager.Instance != null)
        {
            try
            {
                AudioManager.Instance.PlaySFX(currentWeapon.attackSound);
            }
            catch (System.NotImplementedException)
            {
                // AudioManager.PlaySFX not yet implemented
            }
        }
    }

    /// <summary>
    /// Play only visual/audio attack effects without performing hit detection or applying damage.
    /// Useful for AI that applies damage directly but still wants weapon and player attack visuals.
    /// </summary>
    public void PlayAttackVisuals()
    {
        PlayAttackAnimation();
        PlayAttackSound();
    }
    
    /// <summary>
    /// Updates the weapon visual by destroying the old visual and instantiating the new weapon prefab.
    /// </summary>
    private void UpdateWeaponVisual()
    {
        if (weaponVisual != null)
        {
            Destroy(weaponVisual);
        }
        
        if (currentWeapon?.weaponPrefab != null)
        {
            weaponVisual = Instantiate(currentWeapon.weaponPrefab, transform);
            weaponVisual.transform.localPosition = Vector3.zero;
            weaponVisual.transform.localRotation = Quaternion.identity;
            weaponAnimator = weaponVisual.GetComponent<Animator>();
        }
    }
}
