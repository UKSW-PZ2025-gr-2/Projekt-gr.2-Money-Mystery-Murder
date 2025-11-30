using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int damage = 50;
    [SerializeField] private float fuseTimer = 2f;

    private float _timeLeft;

    private void Start()
    {
        // TODO: Logic - start countdown
        throw new System.NotImplementedException();
    }

    private void Update()
    {
        // TODO: Logic - tick fuse and call Explode when <= 0
        throw new System.NotImplementedException();
    }

    public void Explode()
    {
        // TODO: Logic - overlap circle, apply damage, play SFX/VFX, destroy
        throw new System.NotImplementedException();
    }
}
