using UnityEngine;

/// <summary>
/// Abstract base class for all weapon controllers. Defines core weapon properties and attack interface.
/// Used by <see cref="Player"/> to execute attacks.
/// </summary>
public abstract class WeaponController : MonoBehaviour
{
    /// <summary>
    /// Damage dealt per attack.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] protected int damage;
    
    /// <summary>
    /// Cooldown duration between attacks in seconds.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] protected float cooldown;
    
    /// <summary>
    /// Maximum attack range in Unity units.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] protected float range;

    /// <summary>Time of the last attack (in Time.time).</summary>
    protected float lastAttackTime;
    
    /// <summary>The <see cref="Player"/> that owns this weapon.</summary>
    protected Player owner;

    /// <summary>Gets the weapon's damage value.</summary>
    public int Damage => damage;
    
    /// <summary>Gets the weapon's cooldown duration.</summary>
    public float Cooldown => cooldown;
    
    /// <summary>Gets the weapon's attack range.</summary>
    public float Range => range;

    /// <summary>
    /// Initializes the weapon controller with the owning <see cref="Player"/>.
    /// </summary>
    /// <param name="player">The player that owns this weapon.</param>
    public void Initialize(Player player)
    {
        // Set the owning player for this weapon so attacks can reference position/direction
        owner = player;

        // Allow immediate attack after initialization (optional): set lastAttackTime
        // so Time.time - lastAttackTime >= cooldown.
        lastAttackTime = Time.time - cooldown;
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes to perform weapon-specific attack logic.
    /// </summary>
    public abstract void Attack();
}
