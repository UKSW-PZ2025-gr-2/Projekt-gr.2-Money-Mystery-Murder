using UnityEngine;

/// <summary>
/// Scriptable definition of a player ability used by Player.cs.
/// Keep minimal fields required by existing gameplay code.
/// </summary>
[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "Ability")]
public class Ability : ScriptableObject
{
    [Header("Meta")]
    /// <summary>
    /// The display name of the ability shown in UI.
    /// </summary>
    public string displayName;
    
    /// <summary>
    /// Description text explaining what the ability does.
    /// </summary>
    [TextArea]
    public string description;
    
    /// <summary>
    /// Icon sprite displayed in hotbar and shop UI.
    /// </summary>
    public Sprite icon;

    [Header("Economy")]
    /// <summary>
    /// The cost to learn/purchase this ability. Referenced by Player.LearnAbility.
    /// </summary>
    public int cost = 0;

    [Header("Timing")]
    /// <summary>
    /// Duration of the ability effect in seconds. Used by gameplay when activating abilities.
    /// </summary>
    public float duration = 5f;
    
    /// <summary>
    /// Cooldown time in seconds before the ability can be used again. Used by cooldown tracking in Player.
    /// </summary>
    public float cooldown = 10f;

    [Header("Effect")]
    /// <summary>
    /// Simple categorization of the ability type for game logic and UI.
    /// </summary>
    public AbilityKind kind = AbilityKind.None;
    
    /// <summary>
    /// Generic strength value for the ability effect (e.g., speed multiplier, heal amount, etc.).
    /// </summary>
    public float magnitude = 1f;
}

/// <summary>
/// Defines the type of ability effect.
/// </summary>
public enum AbilityKind
{
    /// <summary>
    /// No effect or unassigned ability type.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Temporarily increases player movement speed.
    /// </summary>
    SpeedBoost = 1,
    
    /// <summary>
    /// Makes the player invisible to others.
    /// </summary>
    Invisibility = 2,
    
    /// <summary>
    /// Restores player health.
    /// </summary>
    Heal = 3,
    
    /// <summary>
    /// Deals damage to enemies.
    /// </summary>
    Damage = 4,
    
    /// <summary>
    /// Custom ability type for special implementations.
    /// </summary>
    Custom = 100
}
