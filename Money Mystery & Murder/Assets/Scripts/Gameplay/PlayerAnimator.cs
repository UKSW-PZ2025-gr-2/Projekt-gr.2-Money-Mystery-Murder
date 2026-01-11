using UnityEngine;

/// <summary>
/// Controls player animations for movement, combat, and death states.
/// Interfaces with Unity's Animator component using parameters: isWalking, Attack, Hit, Death, isDead.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    /// <summary>
    /// Reference to the Animator component.
    /// </summary>
    private Animator animator;
    
    /// <summary>
    /// Tracks whether the Animator has an "Attack" trigger parameter.
    /// </summary>
    private bool hasAttackParam;
    
    /// <summary>
    /// Tracks whether the Animator has a "Hit" trigger parameter.
    /// </summary>
    private bool hasHitParam;
    
    /// <summary>
    /// Tracks whether the Animator has a "Death" trigger parameter.
    /// </summary>
    private bool hasDeathParam;
    
    /// <summary>
    /// Tracks whether the Animator has an "isDead" bool parameter.
    /// </summary>
    private bool hasIsDeadParam;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Initializes the Animator and caches parameter availability.
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            CacheAnimatorParameters();
        }
    }

    /// <summary>
    /// Caches which animation parameters are available in the Animator.
    /// Logs warnings for missing parameters.
    /// </summary>
    private void CacheAnimatorParameters()
    {
        foreach (var p in animator.parameters)
        {
            if (p.name == "Attack") hasAttackParam = true;
            else if (p.name == "Hit") hasHitParam = true;
            else if (p.name == "Death") hasDeathParam = true;
            else if (p.name == "isDead") hasIsDeadParam = true;
        }
        
        if (!hasAttackParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Attack' trigger parameter.");
        if (!hasHitParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Hit' trigger parameter.");
        if (!hasDeathParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'Death' trigger parameter.");
        if (!hasIsDeadParam)
            Debug.LogWarning("[PlayerAnimator] Animator on " + gameObject.name + " has no 'isDead' bool parameter.");
    }

    /// <summary>
    /// Sets the movement animation state based on whether the player is moving.
    /// </summary>
    /// <param name="isMoving">True if the player is currently moving, false otherwise.</param>
    public void SetMovementState(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool("isWalking", isMoving);
    }

    /// <summary>
    /// Triggers the attack animation.
    /// </summary>
    public void TriggerAttack()
    {
        if (animator == null) return;
        if (hasAttackParam)
        {
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Triggers the hit/damage animation.
    /// </summary>
    public void TriggerHit()
    {
        if (animator == null) return;
        if (hasHitParam)
        {
            animator.SetTrigger("Hit");
        }
    }

    /// <summary>
    /// Triggers the death animation and sets the permanent death state.
    /// </summary>
    public void TriggerDeath()
    {
        if (animator == null) return;
        
        // Set the boolean to permanently stay in death state
        if (hasIsDeadParam)
        {
            animator.SetBool("isDead", true);
        }
        
        // Also trigger the death animation
        if (hasDeathParam)
        {
            animator.SetTrigger("Death");
        }
    }
}
