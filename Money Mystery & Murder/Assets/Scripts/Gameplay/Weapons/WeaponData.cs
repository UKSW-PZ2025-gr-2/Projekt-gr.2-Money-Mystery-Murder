using UnityEngine;

/// <summary>
/// Defines the type of weapon attack behavior.
/// </summary>
public enum WeaponType
{
    Melee,
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
    public string displayName = "Weapon";
    public Sprite icon;
    [TextArea] public string description;
    
    [Header("Economy")]
    public int cost = 0;
    
    [Header("Combat Stats")]
    public WeaponType weaponType = WeaponType.Melee;
    public int damage = 10;
    public float cooldown = 1f;
    public float range = 1f;
    
    [Header("Melee Settings")]
    [Tooltip("Arc angle in degrees for melee hit detection")]
    public float hitArcDegrees = 90f;
    [Tooltip("Number of raycasts for melee spread detection")]
    public int raycastSpread = 5;
    
    [Header("Ranged Settings")]
    public bool usesAmmo = false;
    public int maxAmmo = 0;
    public float projectileSpeed = 20f;
    public GameObject projectilePrefab;
    
    [Header("Visual & Audio")]
    public GameObject weaponPrefab;
    public GameObject hitEffectPrefab;
    public AnimationClip attackAnimation;
    public AudioClip attackSound;
}
