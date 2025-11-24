using UnityEngine;

public class Rifle : RangedWeapon
{
    [Header("Rifle Stats Overrides")]
    [SerializeField] private int overrideDamage = 15;
    [SerializeField] private float overrideCooldown = 0.1f; // high fire rate
    [SerializeField] private float overrideRange = 12f;
    [SerializeField] private float bulletSpreadDegrees = 4f;

    private void Awake()
    {
        // TODO: Logic - apply overrides to base stats
        throw new System.NotImplementedException();
    }

    private void Update()
    {
        // TODO: Logic - automatic fire while input held
        throw new System.NotImplementedException();
    }

    public override void Attack()
    {
        // TODO: Logic - spawn projectile or raycast with spread and play SFX
        throw new System.NotImplementedException();
    }
}
