using UnityEngine;

/// <summary>
/// Melee weapon controller that implements area-of-effect hit detection for close-range attacks.
/// Inherits from <see cref="WeaponController"/>.
/// </summary>
public class MeleeWeapon : WeaponController
{
    /// <summary>
    /// Arc angle in degrees defining the melee hit zone.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float hitArcDegrees = 90f;
    
    /// <summary>
    /// Radius of the melee hit detection area.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float hitRadius = 1f;

    /// <summary>
    /// Performs a melee attack using overlap detection. Not yet fully implemented.
    /// </summary>
    public override void Attack()
    {
        // TODO: Logic - overlap circle/arc for melee hit
        throw new System.NotImplementedException();
    }
}
