using UnityEngine;

public class RangedWeapon : WeaponController
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzleTransform;

    public override void Attack()
    {
        // TODO: Logic - raycast or spawn projectile and play muzzle VFX
        throw new System.NotImplementedException();
    }
}
