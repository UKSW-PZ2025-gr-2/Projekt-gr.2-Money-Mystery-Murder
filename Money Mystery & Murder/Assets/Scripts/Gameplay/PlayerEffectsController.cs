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
        // TODO: Logic - add or modify effect entry
        throw new System.NotImplementedException();
    }

    public void UpdateEffects()
    {
        // TODO: Logic - tick timers and expire
        throw new System.NotImplementedException();
    }

    public void SetInvisibility(bool isVisible)
    {
        // TODO: Logic - adjust renderer/material
        throw new System.NotImplementedException();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        // TODO: Logic - modify movement speed
        throw new System.NotImplementedException();
    }
}
