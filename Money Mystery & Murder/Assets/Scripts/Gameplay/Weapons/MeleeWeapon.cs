using UnityEngine;

public class MeleeWeapon : WeaponController
{
    [SerializeField] private float hitArcDegrees = 90f;
    [SerializeField] private float hitRadius = 1f;

    public override void Attack()
    {
        // TODO: Logic - overlap circle/arc for melee hit
        throw new System.NotImplementedException();
    }
}
