using UnityEngine;

/// <summary>
/// Defines the type of weapon attack behavior.
/// </summary>
public enum WeaponType
{
    /// <summary>
    /// Close-range weapon that uses arc-based hit detection.
    /// </summary>
    Melee,
    
    /// <summary>
    /// Long-range weapon that fires projectiles.
    /// </summary>
    Ranged
}

/// <summary>
/// Complete weapon definition as a single data type. Use this for inventory, shop, and combat.
/// Replaces the old Weapon ScriptableObject and WeaponController hierarchy.
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data", order = 10)]
public class WeaponData : ScriptableObject
{
    [Header("Display")]
    /// <summary>
    /// The display name of the weapon shown in UI.
    /// </summary>
    public string displayName = "Weapon";
    
    /// <summary>
    /// Icon sprite displayed in hotbar and shop UI.
    /// </summary>
    public Sprite icon;
    
    /// <summary>
    /// Description text for the weapon.
    /// </summary>
    [TextArea] public string description;
    
    [Header("Economy")]
    /// <summary>
    /// The cost to purchase this weapon in the shop.
    /// </summary>
    public int cost = 0;
    
    [Header("Combat Stats")]
    /// <summary>
    /// The type of weapon (Melee or Ranged).
    /// </summary>
    public WeaponType weaponType = WeaponType.Melee;
    
    /// <summary>
    /// The damage dealt by this weapon on hit.
    /// </summary>
    public int damage = 10;
    
    /// <summary>
    /// The cooldown time in seconds between attacks.
    /// </summary>
    public float cooldown = 1f;
    
    /// <summary>
    /// The effective range of the weapon in world units.
    /// </summary>
    public float range = 1f;
    
    [Header("Melee Settings")]
    /// <summary>
    /// Arc angle in degrees for melee hit detection.
    /// </summary>
    [Tooltip("Arc angle in degrees for melee hit detection")]
    public float hitArcDegrees = 90f;
    
    /// <summary>
    /// Number of raycasts for melee spread detection.
    /// </summary>
    [Tooltip("Number of raycasts for melee spread detection")]
    public int raycastSpread = 5;
    
    [Header("Ranged Settings")]
    /// <summary>
    /// Whether this ranged weapon uses limited ammunition.
    /// </summary>
    public bool usesAmmo = false;
    
    /// <summary>
    /// Maximum ammunition capacity for this weapon.
    /// </summary>
    public int maxAmmo = 0;
    
    /// <summary>
    /// The speed of projectiles fired by this weapon.
    /// </summary>
    public float projectileSpeed = 20f;
    
    /// <summary>
    /// The projectile prefab spawned when this weapon fires.
    /// </summary>
    public GameObject projectilePrefab;
    
    [Header("Visual & Audio")]
    /// <summary>
    /// The visual prefab of the weapon displayed on the player.
    /// </summary>
    public GameObject weaponPrefab;
    
    /// <summary>
    /// The visual effect prefab spawned on successful hits.
    /// </summary>
    public GameObject hitEffectPrefab;
    
    /// <summary>
    /// The animation clip played when attacking with this weapon.
    /// </summary>
    public AnimationClip attackAnimation;
    
    /// <summary>
    /// The audio clip played when attacking with this weapon.
    /// </summary>
    public AudioClip attackSound;
}
