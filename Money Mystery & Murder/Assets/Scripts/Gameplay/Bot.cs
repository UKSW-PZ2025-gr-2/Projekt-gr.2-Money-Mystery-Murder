using UnityEngine;

/// <summary>
/// Bot player that inherits from <see cref="Player"/> but does not announce its role.
/// Used for AI-controlled players in the game.
/// </summary>
public class Bot : Player
{
    /// <summary>
    /// Initializes bot state, assigns role from <see cref="GameManager"/>.
    /// Overrides <see cref="Player.Start"/> to skip role announcement.
    /// </summary>
    protected override void Start()
    {
        // Auto-heal if configured
        if (CurrentHealth < MaxHealth)
        {
            Heal(MaxHealth - CurrentHealth);
        }
        
        // Assign role from GameManager if no role is set
        if (Role == PlayerRole.None && GameManager.Instance != null)
        {
            SetRole(GameManager.Instance.PickRandomRoleFromPool());
        }
        
        // Skip roleAnnouncer.ShowRole() - bots don't announce their role
        
        Debug.Log($"[Bot] {gameObject.name} initialized with role: {Role}");
    }
}
