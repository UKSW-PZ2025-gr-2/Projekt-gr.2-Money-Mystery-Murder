using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the type of effect that can be applied to a player.
/// </summary>
public enum EffectType
{
    /// <summary>
    /// Makes the player invisible to others.
    /// </summary>
    Invisibility,
    
    /// <summary>
    /// Increases or modifies the player's movement speed.
    /// </summary>
    Speed
}

/// <summary>
/// Manages temporary effects applied to the player such as speed boosts and invisibility.
/// Handles effect duration, stacking, and removal.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerEffectsController : MonoBehaviour
{
    /// <summary>
    /// Internal class representing an active effect with duration and value.
    /// </summary>
    private class ActiveEffect
    {
        /// <summary>
        /// The type of effect.
        /// </summary>
        public EffectType Type;
        
        /// <summary>
        /// Remaining duration of the effect in seconds.
        /// </summary>
        public float TimeRemaining;
        
        /// <summary>
        /// The value/magnitude of the effect (e.g., speed multiplier).
        /// </summary>
        public float Value;
    }

    /// <summary>
    /// List of currently active effects on the player.
    /// </summary>
    private readonly List<ActiveEffect> _activeEffects = new();
    
    /// <summary>
    /// The SpriteRenderer used for visual effects like invisibility.
    /// </summary>
    [SerializeField] private SpriteRenderer targetRenderer;
    
    /// <summary>
    /// The PlayerMovement component for applying speed effects.
    /// </summary>
    [SerializeField] private PlayerMovement movement;

    /// <summary>
    /// Reference to the Player component.
    /// </summary>
    private Player _player;
    
    /// <summary>
    /// The base speed multiplier to restore after speed effects end.
    /// </summary>
    private float _baseSpeedMultiplier = 1f;
    
    /// <summary>
    /// Tracks whether the player was visible before invisibility effect.
    /// </summary>
    private bool _wasVisible = true;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Initializes component references.
    /// </summary>
    private void Awake()
    {
        _player = GetComponent<Player>();
        
        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }
        
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Applies an effect to the player. If an effect of the same type exists, it refreshes its duration and value.
    /// </summary>
    /// <param name="type">The type of effect to apply.</param>
    /// <param name="duration">Duration of the effect in seconds.</param>
    /// <param name="value">The magnitude/value of the effect.</param>
    public void ApplyEffect(EffectType type, float duration, float value)
    {
        ActiveEffect existing = FindActiveEffect(type);
        
        if (existing != null)
        {
            RefreshEffect(existing, duration, value);
        }
        else
        {
            AddNewEffect(type, duration, value);
        }

        ApplyEffectImmediate(type, value);
    }

    /// <summary>
    /// Updates all active effects, reducing their remaining time and removing expired effects.
    /// Should be called every frame from Player.Update().
    /// </summary>
    public void UpdateEffects()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = _activeEffects[i];
            effect.TimeRemaining -= Time.deltaTime;

            if (effect.TimeRemaining <= 0f)
            {
                RemoveEffect(effect);
                _activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Checks if the player has an active effect of the specified type.
    /// </summary>
    /// <param name="type">The type of effect to check for.</param>
    /// <returns>True if the effect is active, false otherwise.</returns>
    public bool HasActiveEffect(EffectType type)
    {
        return FindActiveEffect(type) != null;
    }

    /// <summary>
    /// Gets the remaining duration of an active effect.
    /// </summary>
    /// <param name="type">The type of effect to query.</param>
    /// <returns>The remaining time in seconds, or 0 if the effect is not active.</returns>
    public float GetEffectTimeRemaining(EffectType type)
    {
        ActiveEffect effect = FindActiveEffect(type);
        return effect?.TimeRemaining ?? 0f;
    }

    /// <summary>
    /// Finds an active effect by type.
    /// </summary>
    /// <param name="type">The type of effect to find.</param>
    /// <returns>The active effect, or null if not found.</returns>
    private ActiveEffect FindActiveEffect(EffectType type)
    {
        return _activeEffects.Find(e => e.Type == type);
    }

    /// <summary>
    /// Refreshes an existing effect with new duration and value.
    /// </summary>
    /// <param name="effect">The effect to refresh.</param>
    /// <param name="duration">New duration in seconds.</param>
    /// <param name="value">New effect value.</param>
    private void RefreshEffect(ActiveEffect effect, float duration, float value)
    {
        effect.TimeRemaining = duration;
        effect.Value = value;
    }

    /// <summary>
    /// Adds a new effect to the active effects list.
    /// </summary>
    /// <param name="type">The type of effect.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="value">Effect value/magnitude.</param>
    private void AddNewEffect(EffectType type, float duration, float value)
    {
        var newEffect = new ActiveEffect
        {
            Type = type,
            TimeRemaining = duration,
            Value = value
        };
        _activeEffects.Add(newEffect);
    }

    /// <summary>
    /// Applies an effect immediately to the player.
    /// </summary>
    /// <param name="type">The type of effect to apply.</param>
    /// <param name="value">The effect value/magnitude.</param>
    private void ApplyEffectImmediate(EffectType type, float value)
    {
        switch (type)
        {
            case EffectType.Invisibility:
                SetInvisibility(false);
                break;
            
            case EffectType.Speed:
                SetSpeedMultiplier(value);
                break;
        }
    }

    /// <summary>
    /// Removes an effect and restores the player's default state.
    /// </summary>
    /// <param name="effect">The effect to remove.</param>
    private void RemoveEffect(ActiveEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.Invisibility:
                SetInvisibility(true);
                break;
            
            case EffectType.Speed:
                SetSpeedMultiplier(_baseSpeedMultiplier);
                break;
        }
    }

    /// <summary>
    /// Sets the player's visibility by adjusting sprite alpha.
    /// </summary>
    /// <param name="isVisible">True to make visible, false to make invisible.</param>
    private void SetInvisibility(bool isVisible)
    {
        if (targetRenderer == null) return;

        Color color = targetRenderer.color;
        color.a = isVisible ? 1f : 0f;
        targetRenderer.color = color;
    }

    /// <summary>
    /// Sets the player's movement speed multiplier.
    /// </summary>
    /// <param name="multiplier">The speed multiplier to apply.</param>
    private void SetSpeedMultiplier(float multiplier)
    {
        if (movement == null) return;

        movement.SetSpeedMultiplier(multiplier);
    }
}
