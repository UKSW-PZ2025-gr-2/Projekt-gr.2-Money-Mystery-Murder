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

    private readonly List<ActiveEffect> _activeEffects = new();
    
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private PlayerMovement movement;

    private Player _player;
    private float _baseSpeedMultiplier = 1f;
    private bool _wasVisible = true;

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

    public bool HasActiveEffect(EffectType type)
    {
        return FindActiveEffect(type) != null;
    }

    public float GetEffectTimeRemaining(EffectType type)
    {
        ActiveEffect effect = FindActiveEffect(type);
        return effect?.TimeRemaining ?? 0f;
    }

    private ActiveEffect FindActiveEffect(EffectType type)
    {
        return _activeEffects.Find(e => e.Type == type);
    }

    private void RefreshEffect(ActiveEffect effect, float duration, float value)
    {
        effect.TimeRemaining = duration;
        effect.Value = value;
    }

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

    private void SetInvisibility(bool isVisible)
    {
        if (targetRenderer == null) return;

        Color color = targetRenderer.color;
        color.a = isVisible ? 1f : 0f;
        targetRenderer.color = color;
    }

    private void SetSpeedMultiplier(float multiplier)
    {
        if (movement == null) return;

        movement.SetSpeedMultiplier(multiplier);
    }
}
