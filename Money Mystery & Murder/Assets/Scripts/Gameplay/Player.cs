using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    [SerializeField] private Weapon equippedWeapon; 
    [SerializeField] private List<Weapon> ownedWeapons = new(); 
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
    [Header("Weapon Socket")]
    [Tooltip("Optional transform to parent equipped weapons to (e.g. HipSocket or Hand). If null, weapon will be parented to the player root.")]
    [SerializeField] private Transform weaponSocket;
    [Tooltip("Local position applied to an equipped weapon when parented (used if no socket or to offset inside socket).")]
    [SerializeField] private Vector3 weaponLocalPosition = new Vector3(1.2f, 0.6f, 0.2f);
    [Tooltip("Local Euler angles applied to an equipped weapon when parented.")]
    [SerializeField] private Vector3 weaponLocalEuler = new Vector3(0f, 90f, 0f);
    // If we instantiate a runtime copy of a prefab assigned in inspector, track it here so we can clean it up later
    private WeaponController _runtimeWeaponInstance;

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

    public Weapon EquippedWeapon => equippedWeapon;
    public IReadOnlyList<Weapon> OwnedWeapons => ownedWeapons;
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

        // If a WeaponController is already assigned in the inspector, initialize it now
        if (currentWeapon != null)
        {
            SetCurrentWeapon(currentWeapon);
        }
    }

    void Update()
    {
        TickActiveAbility();
        TickCooldowns();
        if (effectsController != null)
        {
            effectsController.UpdateEffects();
        }

        // Keep weapon transform synced with current position/rotation (so it moves with player and changes in inspector take effect immediately)
        if (currentWeapon != null)
        {
            var weaponGO = currentWeapon.gameObject;
            var parent = (weaponSocket != null) ? weaponSocket : this.transform;
            // Only update parent position if parent reference changed
            if (weaponGO.transform.parent != parent)
            {
                weaponGO.transform.SetParent(parent, false);
            }
            // Update base position/rotation. Animation can offset from this base position.
            weaponGO.transform.localPosition = weaponLocalPosition;
            weaponGO.transform.localRotation = Quaternion.Euler(weaponLocalEuler);
        }

        // Input: support both the old Input Manager and the new Input System
#if ENABLE_INPUT_SYSTEM
        bool pressed = false;
        if (Mouse.current != null)
        {
            pressed = Mouse.current.leftButton.wasPressedThisFrame;
        }
        else
        {
            // Fallback to old input if Mouse.current is unexpectedly null
            pressed = Input.GetMouseButtonDown(0);
        }
        if (pressed)
        {
            PerformAttack();
        }
#else
        // Old Input Manager
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
#endif
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
    public void AcquireWeapon(Weapon weapon)
    {
        // 1. Null/duplicate check
        // 2. Add to ownedWeapons
        // 3. Auto-equip if none equipped
        if (weapon == null || ownedWeapons.Contains(weapon)) return;    
        ownedWeapons.Add(weapon);
        if (equippedWeapon == null) equippedWeapon = weapon;
    }

    public void EquipWeapon(Weapon weapon)
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

        // Decrease remaining time and end ability when it expires
        _activeAbilityTimeLeft -= Time.deltaTime;
        if (_activeAbilityTimeLeft <= 0f)
        {
            EndActiveAbility();
        }
    }

    private void EndActiveAbility()
    {
        // 1. Deactivate effects
        // 2. Clear active ability
        // Basic cleanup for active ability
        _activeAbility = null;
        _activeAbilityTimeLeft = 0f;
    }

    private void TickCooldowns()
    {
        // 1. Iterate keys
        // 2. Decrement >0 entries
        // 3. Clamp at 0
        if (_cooldowns.Count == 0) return;

        var keys = new List<AbilityDefinition>(_cooldowns.Keys);
        foreach (var k in keys)
        {
            if (_cooldowns[k] > 0f)
            {
                _cooldowns[k] = Mathf.Max(0f, _cooldowns[k] - Time.deltaTime);
            }
        }
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
        // Clean up previously instantiated runtime weapon if any
        if (_runtimeWeaponInstance != null)
        {
            try { Destroy(_runtimeWeaponInstance.gameObject); }
            catch { }
            _runtimeWeaponInstance = null;
        }

        if (weapon == null)
        {
            currentWeapon = null;
            return;
        }

        // Always instantiate the weapon to ensure we get a scene instance (not a prefab asset).
        // This prevents "Setting the parent of a transform which resides in a Prefab Asset" errors.
        var weaponGO = weapon.gameObject;
        var instantiatedGO = Instantiate(weaponGO);
        var instance = instantiatedGO.GetComponent<WeaponController>();
        _runtimeWeaponInstance = instance;

        if (instance == null)
        {
            Debug.LogError("SetCurrentWeapon: instantiated weapon GameObject has no WeaponController component");
            Destroy(instantiatedGO);
            currentWeapon = null;
            return;
        }

        currentWeapon = instance;
        currentWeapon.Initialize(this);

        // Parent the weapon GameObject to the player so it's visible and moves with the player.
        try
        {
            var go = currentWeapon.gameObject;
            var parent = (weaponSocket != null) ? weaponSocket : this.transform;
            go.transform.SetParent(parent, false);
            // apply configurable local transform so you can move the knife further from center in the Inspector
            go.transform.localPosition = weaponLocalPosition;
            go.transform.localRotation = Quaternion.Euler(weaponLocalEuler);
        }
        catch (System.Exception)
        {
            // ignore any parenting errors in editor/runtime
        }

    }

    public void PerformAttack()
    {
        if (currentWeapon == null) return;

        currentWeapon.Attack();

        // Trigger player attack animation if available
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttack();
        }
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
