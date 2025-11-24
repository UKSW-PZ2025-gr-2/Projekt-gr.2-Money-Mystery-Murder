using UnityEngine;

public class Katana : MeleeWeapon
{
    [Header("Katana Stats Overrides")]
    [SerializeField] private int overrideDamage = 30;
    [SerializeField] private float overrideCooldown = 0.6f;
    [SerializeField] private float overrideRange = 1.5f;
    [SerializeField] private float overrideHitArcDegrees = 140f;

    private void Awake()
    {
        // TODO: Logic - apply overrides to base stats
        throw new System.NotImplementedException();
    }

    public override void Attack()
    {
        // TODO: Logic - wide slash animation and cleave hit detection
        throw new System.NotImplementedException();
    }
}
