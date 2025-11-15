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

    [Header("Effects While Active")]
    [Tooltip("Movement speed multiplier while active (1 = no change).")]
    public float speedMultiplier = 1f;
    [Tooltip("Allow passing through walls by changing layer.")]
    public bool allowWallPass = false;
    [Tooltip("Layer index to switch to while active (e.g. layer without wall collisions). -1 keeps current layer.")]
    public int wallPassLayer = -1;

    // Hotbar binding will be handled by UI/system later at runtime.
}
