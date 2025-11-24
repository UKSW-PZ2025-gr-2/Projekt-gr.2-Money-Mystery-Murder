using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void SetMovementState(bool isMoving)
    {
        // TODO: Logic - set animator parameters
        throw new System.NotImplementedException();
    }

    public void TriggerAttack()
    {
        // TODO: Logic - trigger attack animation
        throw new System.NotImplementedException();
    }

    public void TriggerHit()
    {
        // TODO: Logic - trigger hit reaction
        throw new System.NotImplementedException();
    }

    public void TriggerDeath()
    {
        // TODO: Logic - trigger death animation
        throw new System.NotImplementedException();
    }
}
