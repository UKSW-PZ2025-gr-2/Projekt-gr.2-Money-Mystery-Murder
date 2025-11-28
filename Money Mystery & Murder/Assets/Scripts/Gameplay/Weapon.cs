using UnityEngine;

/// <summary>
/// ScriptableObject that defines a weapon's properties including damage, fire rate, range, and ammo settings.
/// Set this in the Unity Inspector.
/// </summary>
[CreateAssetMenu(fileName = "Weapon", menuName = "Game/Weapon", order = 10)]
public class Weapon : ScriptableObject
{
    /// <summary>
    /// Display name of the weapon shown in UI.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("General")] public string displayName = "Weapon";
    
    /// <summary>
    /// Icon sprite representing this weapon in inventory and UI.
    /// Set this in the Unity Inspector.
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// Base damage dealt per attack.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Combat Stats")] public int damage = 10;
    
    /// <summary>
    /// Fire rate in attacks per second.
    /// Set this in the Unity Inspector.
    /// </summary>
    public float fireRate = 1f;
    
    /// <summary>
    /// Maximum effective range in Unity units.
    /// Set this in the Unity Inspector.
    /// </summary>
    public float range = 10f;

    /// <summary>
    /// Whether this weapon uses ammunition.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Ammo")] public bool usesAmmo = false;
    
    /// <summary>
    /// Maximum ammunition capacity when usesAmmo is true.
    /// Set this in the Unity Inspector.
    /// </summary>
    public int maxAmmo = 0;

    /// <summary>
    /// Descriptive text explaining the weapon's characteristics.
    /// Set this in the Unity Inspector.
    /// </summary>
    [TextArea] public string description;
}
