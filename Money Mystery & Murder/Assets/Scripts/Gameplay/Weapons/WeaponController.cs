using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    [SerializeField] protected int damage;
    [SerializeField] protected float cooldown;
    [SerializeField] protected float range;

    protected float lastAttackTime;
    protected Player owner;

    public int Damage => damage;
    public float Cooldown => cooldown;
    public float Range => range;

    public void Initialize(Player player)
    {
        // Set the owning player for this weapon so attacks can reference position/direction
        owner = player;

        // Allow immediate attack after initialization (optional): set lastAttackTime
        // so Time.time - lastAttackTime >= cooldown.
        lastAttackTime = Time.time - cooldown;
    }

    public abstract void Attack();
}
