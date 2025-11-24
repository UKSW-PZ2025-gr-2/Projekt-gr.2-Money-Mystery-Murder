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
        // TODO: Logic - set owner
        throw new System.NotImplementedException();
    }

    public abstract void Attack();
}
