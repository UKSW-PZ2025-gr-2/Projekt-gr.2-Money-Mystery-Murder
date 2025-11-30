using System.Collections.Generic;
using UnityEngine;

public enum PlayerRole
{
    None,
    Civilian,
    Detective,
    Murderer
}

public class Player : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100; 
    [SerializeField] private int balance = 0;
    [SerializeField] private PlayerRole role = PlayerRole.None; 
    [SerializeField] private bool isAlive = true;
    [SerializeField] private float visionRange = 5f;

    [Header("Inventory / Progression")]
    [SerializeField] private WeaponDefinition equippedWeapon; 
    [SerializeField] private List<WeaponDefinition> ownedWeapons = new(); 
    [SerializeField] private List<ShopItem> hotbarItems = new(); // snapshot of items relevant to hotbar usage

    [Header("Abilities")]
    [SerializeField] private List<AbilityDefinition> learnedAbilities = new(); 
    private readonly Dictionary<AbilityDefinition, float> _cooldowns = new(); 
    private AbilityDefinition _activeAbility; 
    private float _activeAbilityTimeLeft;

    [Header("UI")]
    [SerializeField] private RoleAnnouncer roleAnnouncer;

    [Header("Effects & Animation")]
    [SerializeField] private PlayerEffectsController effectsController;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Weapon Controller")]
    [SerializeField] private WeaponController currentWeapon;

    [Header("Events / Flags")]
    [SerializeField] private bool autoHealToMaxOnStart = false;

    public int Balance => balance;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public PlayerRole Role => role;
    public bool IsAlive => isAlive;
    public float VisionRange => visionRange;

    public PlayerRole CurrentRole => role;
    public int Money => balance;
    public int CurrentHP => currentHealth;

    public WeaponDefinition EquippedWeapon => equippedWeapon;
    public IReadOnlyList<WeaponDefinition> OwnedWeapons => ownedWeapons;
    public IReadOnlyList<AbilityDefinition> LearnedAbilities => learnedAbilities;
    public AbilityDefinition ActiveAbility => _activeAbility;

    void Awake()
    {
        if (roleAnnouncer == null)
        {
            roleAnnouncer = GetComponentInChildren<RoleAnnouncer>(true);
        }
        if (effectsController == null) effectsController = GetComponent<PlayerEffectsController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
    }

    void OnEnable()
    {
        // Subscribe to global events (e.g., AchievementManager.NotifyEvent callbacks) if needed
    }

    void OnDisable()
    {
        // Unsubscribe from events
    }

    void Start()
    {
        if (autoHealToMaxOnStart) currentHealth = maxHealth;
        if (role == PlayerRole.None && GameManager.Instance != null)
        {
            role = GameManager.Instance.PickRandomRoleFromPool();
        }
        if (roleAnnouncer != null)
        {
            roleAnnouncer.ShowRole(role);
        }
        // Initialize hotbarItems list if needed (could be fixed size or dynamic)
    }

    void Update()
    {
        TickActiveAbility();
        TickCooldowns();
        if (effectsController != null)
        {
            effectsController.UpdateEffects();
        }
    }

    // ----- Currency -----
    public void AddBalance(int amount)
    {
        // 1. Validate amount > 0
        // 2. Add to balance
        // 3. Optionally notify UI / achievements
        if (amount <= 0) return;
        balance += amount;
    }

    public void AddMoney(int amount)
    {
        // 1. Alias for AddBalance (for external calls)
        // 2. Could trigger money-specific events
        throw new System.NotImplementedException();
    }

    public bool SpendBalance(int amount, bool allowNegative = false)
    {
        // 1. Validate amount
        // 2. Check funds if !allowNegative
        // 3. Deduct and return true on success
        if (amount <= 0) return true;
        if (!allowNegative && balance < amount) return false;
        balance -= amount;
        return true;
    }

    // ----- Health -----
    public void TakeDamage(int dmg)
    {
        // 1. Ignore non-positive damage
        // 2. Subtract from currentHealth
        // 3. If <=0 -> Die()
        if (dmg <= 0) return;
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        // 4. Trigger hit animation or blood VFX through animator/VFXManager
    }

    public void Die()
    {
        // 1. Set isAlive=false
        // 2. Trigger death animation
        // 3. Disable movement/input components
        // 4. Notify GameManager for win condition check
        throw new System.NotImplementedException();
    }

    public void Heal(int hp)
    {
        // 1. Validate positive hp
        // 2. Add and clamp to maxHealth
        // 3. Play heal VFX if available
        if (hp <= 0) return;
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
    }

    // ----- Role -----
    public void SetRole(PlayerRole newRole)
    {
        // 1. Assign role
        // 2. Optionally update UI / achievements
        role = newRole;
    }

    // ----- Weapons (Simple Definition-based Inventory) -----
    public void AcquireWeapon(WeaponDefinition weapon)
    {
        // 1. Null/duplicate check
        // 2. Add to ownedWeapons
        // 3. Auto-equip if none equipped
        if (weapon == null || ownedWeapons.Contains(weapon)) return;    
        ownedWeapons.Add(weapon);
        if (equippedWeapon == null) equippedWeapon = weapon;
    }

    public void EquipWeapon(WeaponDefinition weapon)
    {
        // 1. Validate in ownedWeapons
        // 2. Assign equippedWeapon
        // 3. Update active WeaponController if present
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        equippedWeapon = weapon;
    }

    // ----- Abilities API -----
    public bool LearnAbility(AbilityDefinition ability)
    {
        // 1. Validate ability
        // 2. Check cost & spend
        // 3. Add to learnedAbilities and initialize cooldown entry
        if (ability == null || learnedAbilities.Contains(ability)) return false;
        if (!SpendBalance(ability.cost)) return false;
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f;
        return true;
    }

    public bool ActivateAbility(AbilityDefinition ability)
    {
        // 1. Validate ownership
        // 2. Check cooldown
        // 3. End existing active ability if needed
        // 4. Set active ability + timers
        // 5. Trigger effect activation
        // 6. Start cooldown
        throw new System.NotImplementedException();
    }

    public bool ActivateAbilityByIndex(int index)
    {
        // 1. Index bounds check
        // 2. Retrieve ability and call ActivateAbility
        if (index < 0 || index >= learnedAbilities.Count) return false;
        return ActivateAbility(learnedAbilities[index]);
    }

    public float GetCooldownRemaining(AbilityDefinition ability)
    {
        // 1. Look up dictionary; return clamped value
        if (ability == null) return 0f;
        return _cooldowns.TryGetValue(ability, out var t) ? Mathf.Max(0f, t) : 0f;
    }

    public bool IsAbilityActive(AbilityDefinition ability)
    {
        return _activeAbility == ability;
    }

    private void TickActiveAbility()
    {
        // 1. Early return if none
        // 2. Tick duration
        // 3. Tick effects OnTick
        // 4. If expired call EndActiveAbility
        if (_activeAbility == null) return;
        throw new System.NotImplementedException();
    }

    private void EndActiveAbility()
    {
        // 1. Deactivate effects
        // 2. Clear active ability
        throw new System.NotImplementedException();
    }

    private void TickCooldowns()
    {
        // 1. Iterate keys
        // 2. Decrement >0 entries
        // 3. Clamp at 0
        if (_cooldowns.Count == 0) return;
        throw new System.NotImplementedException();
    }

    // ----- Effects Convenience -----
    public void ApplySpeedEffect(float duration, float multiplier)
    {
        // 1. Forward to effectsController.ApplyEffect(EffectType.Speed,...)
        throw new System.NotImplementedException();
    }

    public void ApplyInvisibilityEffect(float duration)
    {
        // 1. Forward to effectsController.ApplyEffect(EffectType.Invisibility,...)
        throw new System.NotImplementedException();
    }

    // ----- Weapon Controller Integration -----
    public void SetCurrentWeapon(WeaponController weapon)
    {
        // 1. Assign currentWeapon
        // 2. Initialize weapon with this player
        // 3. Update animator state
        throw new System.NotImplementedException();
    }

    public void PerformAttack()
    {
        // 1. Check currentWeapon not null
        // 2. Call currentWeapon.Attack()
        // 3. Trigger animator attack
        throw new System.NotImplementedException();
    }

    // ----- Hotbar Item Usage -----
    public void UseItem(ShopItem item)
    {
        // 1. Null check
        // 2. If Weapon -> Equip or SetCurrentWeapon
        // 3. If Upgrade/Consumable -> apply corresponding effect (speed/invisibility etc.)
        // 4. Deduct charges if consumable
        // 5. Trigger UI update
        throw new System.NotImplementedException();
    }
}
