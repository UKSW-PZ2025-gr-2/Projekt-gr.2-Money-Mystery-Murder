using UnityEngine;

// Scriptable definition of a player ability used by Player.cs.
// Keep minimal fields required by existing gameplay code.
[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "Ability")]
public class Ability : ScriptableObject
{
    [Header("Meta")]
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Economy")]
    public int cost = 0; // referenced by Player.LearnAbility

    [Header("Timing")]
    public float duration = 5f; // used by gameplay when activating abilities
    public float cooldown = 10f; // used by cooldown tracking in Player

    [Header("Effect")]
    public AbilityKind kind = AbilityKind.None; // simple categorization for game logic/UI
    public float magnitude = 1f; // generic strength value (e.g., speed multiplier, seconds, etc.)
}

public enum AbilityKind
{
    None = 0,
    SpeedBoost = 1,
    Invisibility = 2,
    Heal = 3,
    Damage = 4,
    Custom = 100
}
