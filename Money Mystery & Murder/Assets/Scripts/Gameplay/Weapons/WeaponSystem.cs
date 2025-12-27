using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unified weapon system that handles all weapon logic based on WeaponData.
/// Replaces the old WeaponController, MeleeWeapon, Knife, and GoldenKnife classes.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Current Weapon")]
    [SerializeField] private WeaponData currentWeapon;
    
    [Header("Hit Detection")]
    [SerializeField] private LayerMask hitLayers = ~0;
    
    [Header("Visual References")]
    [SerializeField] private Animator weaponAnimator;
    
    private Player owner;
    private float lastAttackTime;
    private int currentAmmo;
    private GameObject weaponVisual;
    
    public WeaponData CurrentWeapon => currentWeapon;
    public int CurrentAmmo => currentAmmo;
    
    /// <summary>
    /// Initialize the weapon system with the owning player.
    /// </summary>
    public void Initialize(Player player)
    {
        owner = player;
        lastAttackTime = Time.time - (currentWeapon?.cooldown ?? 0f);
    }
    
    /// <summary>
    /// Equip a new weapon from data.
    /// </summary>
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
    /// </summary>
    public void Attack()
    {
        if (currentWeapon == null) return;
        if (!CanAttack()) return;
        
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
    /// </summary>
    public bool CanAttack()
    {
        if (currentWeapon == null) return false;
        if (owner != null && owner.IsInMinigameOrShop()) return false;
        if (Time.time - lastAttackTime < currentWeapon.cooldown) return false;
        if (currentWeapon.usesAmmo && currentAmmo <= 0) return false;
        return true;
    }
    
    /// <summary>
    /// Get the time remaining until next attack is available.
    /// </summary>
    public float GetCooldownRemaining()
    {
        if (currentWeapon == null) return 0f;
        return Mathf.Max(0f, currentWeapon.cooldown - (Time.time - lastAttackTime));
    }
    
    private void PerformMeleeAttack()
    {
        Vector3 origin = GetAttackOrigin();
        Vector3 forward = transform.forward;
        
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
                    target.TakeDamage(currentWeapon.damage);
                    Debug.Log($"[WeaponSystem] {owner.name} hit {target.name} for {currentWeapon.damage} damage with {currentWeapon.displayName}");
                }
            }
        }
    }
    
    private void PerformRangedAttack()
    {
        Vector3 origin = GetAttackOrigin();
        Vector3 direction = transform.forward;
        
        if (currentWeapon.projectilePrefab != null)
        {
            GameObject projectile = Instantiate(
                currentWeapon.projectilePrefab, 
                origin, 
                Quaternion.LookRotation(direction)
            );
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * currentWeapon.projectileSpeed;
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
                    target.TakeDamage(currentWeapon.damage);
                    Debug.Log($"[WeaponSystem] {owner.name} shot {target.name} for {currentWeapon.damage} damage with {currentWeapon.displayName}");
                }
            }
        }
    }
    
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
    
    private Vector3 GetAttackOrigin()
    {
        return transform.position;
    }
    
    private Player GetPlayerFromCollider(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player == null) player = collider.GetComponentInParent<Player>();
        if (player == null) player = collider.GetComponentInChildren<Player>();
        return player;
    }
    
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
    
    private void PlayAttackAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
    }
    
    private void PlayAttackSound()
    {
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
