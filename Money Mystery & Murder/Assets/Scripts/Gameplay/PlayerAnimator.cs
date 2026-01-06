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
        if (animator == null) return;
        animator.SetBool("IsMoving", isMoving);
    }

    public void TriggerAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlayerAnimator] TriggerAttack called but Animator is null on " + gameObject.name);
            return;
        }

        bool hasParam = false;
        foreach (var p in animator.parameters)
        {
            if (p.name == "Attack") { hasParam = true; break; }
        }

        if (!hasParam)
        {
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Attack' trigger parameter.");
        }

        animator.SetTrigger("Attack");
    }

    public void TriggerHit()
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlayerAnimator] TriggerHit called but Animator is null on " + gameObject.name);
            return;
        }

        bool hasParam = false;
        foreach (var p in animator.parameters)
        {
            if (p.name == "Hit") { hasParam = true; break; }
        }

        if (!hasParam)
        {
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Hit' trigger parameter.");
        }

        animator.SetTrigger("Hit");
    }

    public void TriggerDeath()
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlayerAnimator] TriggerDeath called but Animator is null on " + gameObject.name);
            return;
        }

        bool hasParam = false;
        foreach (var p in animator.parameters)
        {
            if (p.name == "Death") { hasParam = true; break; }
        }

        if (!hasParam)
        {
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Death' trigger parameter.");
        }

        animator.SetTrigger("Death");
    }
}
