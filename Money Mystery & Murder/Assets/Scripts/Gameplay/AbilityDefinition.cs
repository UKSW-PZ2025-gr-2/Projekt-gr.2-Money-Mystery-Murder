using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Game/Ability", order = 12)]
public class AbilityDefinition : ScriptableObject
{
    [Header("General")] public string displayName = "Ability";
    public Sprite icon;
    [TextArea] public string description;

    [Header("Economy")] public int cost = 100; // currency cost to learn/unlock

    [Header("Activation")]
    [Tooltip("Cooldown time after ability use (seconds).")]
    public float cooldown = 5f;     
    [Tooltip("Duration the ability stays active (seconds). 0 = instant effect only.")]
    public float duration = 3f;

    [Header("Effects")]
    [Tooltip("List of modular effects that will be applied when this ability is active.")]
    public List<AbilityEffect> effects = new();
}
