using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Invisibility,
    Speed
}

[RequireComponent(typeof(Player))]
public class PlayerEffectsController : MonoBehaviour
{
    private class ActiveEffect
    {
        public EffectType Type;
        public float TimeRemaining;
        public float Value;
    }

    [SerializeField] private List<ActiveEffect> activeEffects = new();
    [SerializeField] private SpriteRenderer targetRenderer; // for invisibility alpha toggle
    [SerializeField] private PlayerMovement movement;

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void ApplyEffect(EffectType type, float duration, float value)
    {
        // Check if effect already exists, if so, extend/refresh it
        ActiveEffect existing = activeEffects.Find(e => e.Type == type);
        if (existing != null)
        {
            // Refresh duration and update value
            existing.TimeRemaining = duration;
            existing.Value = value;
        }
        else
        {
            // Add new effect
            var newEffect = new ActiveEffect
            {
                Type = type,
                TimeRemaining = duration,
                Value = value
            };
            activeEffects.Add(newEffect);
        }

        // Apply immediate effect
        ApplyEffectImmediate(type, value);
    }

    public void UpdateEffects()
    {
        // Tick down all active effects
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.TimeRemaining -= Time.deltaTime;

            if (effect.TimeRemaining <= 0f)
            {
                // Effect expired, remove it and restore default state
                RemoveEffect(effect);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void SetInvisibility(bool isVisible)
    {
        if (targetRenderer != null)
        {
            // Make player invisible by setting alpha to 0, visible by setting alpha to 1
            Color color = targetRenderer.color;
            color.a = isVisible ? 1f : 0f;
            targetRenderer.color = color;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        if (movement != null)
        {
            movement.SetSpeedMultiplier(multiplier);
        }
    }

    /// <summary>Applies an effect immediately (when first activated or refreshed).</summary>
    private void ApplyEffectImmediate(EffectType type, float value)
    {
        switch (type)
        {
            case EffectType.Invisibility:
                SetInvisibility(false); // false = invisible
                break;
            case EffectType.Speed:
                SetSpeedMultiplier(value);
                break;
        }
    }

    /// <summary>Removes an effect and restores default state.</summary>
    private void RemoveEffect(ActiveEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.Invisibility:
                SetInvisibility(true); // true = visible
                break;
            case EffectType.Speed:
                SetSpeedMultiplier(1f); // restore normal speed
                break;
        }
    }
}
