using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private bool hasAttackParam;
    private bool hasHitParam;
    private bool hasDeathParam;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            CacheAnimatorParameters();
        }
    }

    private void CacheAnimatorParameters()
    {
        foreach (var p in animator.parameters)
        {
            if (p.name == "Attack") hasAttackParam = true;
            else if (p.name == "Hit") hasHitParam = true;
            else if (p.name == "Death") hasDeathParam = true;
        }
        
        if (!hasAttackParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Attack' trigger parameter.");
        if (!hasHitParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Hit' trigger parameter.");
        if (!hasDeathParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Death' trigger parameter.");
    }

    public void SetMovementState(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool("isWalking", isMoving);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        if (hasAttackParam)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TriggerHit()
    {
        if (animator == null) return;
        if (hasHitParam)
        {
            animator.SetTrigger("Hit");
        }
    }

    public void TriggerDeath()
    {
        if (animator == null) return;
        if (hasDeathParam)
        {
            animator.SetTrigger("Death");
        }
    }
}
