using UnityEngine;

public class ThrowableWeapon : WeaponController
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float explosionRadius = 3f;

    public override void Attack()
    {
        // TODO: Logic - instantiate grenade and schedule explosion
        throw new System.NotImplementedException();
    }

    public void Explode()
    {
        // TODO: Logic - area damage + VFXManager.PlayExplosion
        throw new System.NotImplementedException();
    }
}
