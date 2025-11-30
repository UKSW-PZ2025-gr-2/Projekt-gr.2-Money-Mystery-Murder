using UnityEngine;

public class GrenadeWeapon : ThrowableWeapon
{
    [Header("Grenade Weapon Overrides")]
    [SerializeField] private int overrideDamage = 50;
    [SerializeField] private float overrideCooldown = 1.5f;
    [SerializeField] private float overrideRange = 8f; // throw force magnitude placeholder

    private void Awake()
    {
        // TODO: Logic - apply overrides to base stats
        throw new System.NotImplementedException();
    }

    public override void Attack()
    {
        // TODO: Logic - instantiate grenade projectile and apply velocity
        throw new System.NotImplementedException();
    }
}
