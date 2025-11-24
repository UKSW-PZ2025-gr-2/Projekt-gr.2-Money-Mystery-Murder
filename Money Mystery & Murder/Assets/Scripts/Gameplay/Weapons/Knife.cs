using UnityEngine;

public class Knife : MeleeWeapon
{
    [Header("Knife Stats Overrides")]
    [SerializeField] private int overrideDamage = 10;
    [SerializeField] private float overrideCooldown = 0.3f;
    [SerializeField] private float overrideRange = 0.75f;

    private void Awake()
    {
        // TODO: Logic - apply overrides to base stats
        throw new System.NotImplementedException();
    }

    public override void Attack()
    {
        // TODO: Logic - quick stab animation and hit detection
        throw new System.NotImplementedException();
    }
}
