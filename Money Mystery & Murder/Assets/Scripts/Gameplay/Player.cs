using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Enum defining the different player roles.
/// </summary>
public enum PlayerRole
{
    /// <summary>No role assigned.</summary>
    None,
    /// <summary>Civilian role.</summary>
    Civilian,
    /// <summary>Detective role.</summary>
    Detective,
    /// <summary>Murderer role.</summary>
    Murderer
}

/// <summary>
/// Core player controller managing health, balance, inventory, abilities, and combat.
/// Integrates with <see cref="WeaponController"/>, <see cref="RoleAnnouncer"/>, and <see cref="GameManager"/>.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>Maximum health points. Set this in the Unity Inspector.</summary>
    [Header("Core Stats")]
    [SerializeField] private int maxHealth = 100;
    
    /// <summary>Current health points. Set this in the Unity Inspector.</summary>
    [SerializeField] private int currentHealth = 100;
    
    /// <summary>Player's currency balance. Set this in the Unity Inspector.</summary>
    [SerializeField] private int balance = 0;
    
    /// <summary>Player's assigned role (Civilian, Detective, or Murderer). Set this in the Unity Inspector.</summary>
    [SerializeField] private PlayerRole role = PlayerRole.None;
    
    /// <summary>Whether the player is alive. Set this in the Unity Inspector.</summary>
    [SerializeField] private bool isAlive = true;
    
    /// <summary>Vision range for detecting other players. Set this in the Unity Inspector.</summary>
    [SerializeField] private float visionRange = 5f;

    /// <summary>Currently equipped <see cref="Weapon"/> definition. Set this in the Unity Inspector.</summary>
    [Header("Inventory / Progression")]
    [SerializeField] private Weapon equippedWeapon;
    
    /// <summary>List of owned <see cref="Weapon"/> definitions. Set this in the Unity Inspector.</summary>
    [SerializeField] private List<Weapon> ownedWeapons = new();
    
    /// <summary>Snapshot of items relevant to hotbar usage. Set this in the Unity Inspector.</summary>
    [SerializeField] private List<ShopItem> hotbarItems = new();

    /// <summary>List of learned abilities. Set this in the Unity Inspector.</summary>
    [Header("Abilities")]
    [SerializeField] private List<AbilityDefinition> learnedAbilities = new();
    
    /// <summary>Dictionary tracking cooldown times for learned abilities.</summary>
    private readonly Dictionary<AbilityDefinition, float> _cooldowns = new();
    
    /// <summary>The currently active ability, if any.</summary>
    private AbilityDefinition _activeAbility;
    
    /// <summary>Remaining time for the active ability.</summary>
    private float _activeAbilityTimeLeft;

    /// <summary>Reference to the <see cref="RoleAnnouncer"/> UI component. Set this in the Unity Inspector.</summary>
    [Header("UI")]
    [SerializeField] private RoleAnnouncer roleAnnouncer;

    /// <summary>Reference to the player effects controller. Set this in the Unity Inspector.</summary>
    [Header("Effects & Animation")]
    [SerializeField] private PlayerEffectsController effectsController;
    
    /// <summary>Reference to the player animator. Set this in the Unity Inspector.</summary>
    [SerializeField] private PlayerAnimator playerAnimator;

    /// <summary>Current <see cref="WeaponController"/> instance. Set this in the Unity Inspector.</summary>
    [Header("Weapon Controller")]
    [SerializeField] private WeaponController currentWeapon;
    
    /// <summary>Optional transform to parent equipped weapons to (e.g. HipSocket or Hand). If null, weapon will be parented to the player root. Set this in the Unity Inspector.</summary>
    [Header("Weapon Socket")]
    [Tooltip("Optional transform to parent equipped weapons to (e.g. HipSocket or Hand). If null, weapon will be parented to the player root.")]
    [SerializeField] private Transform weaponSocket;
    
    /// <summary>Local position applied to an equipped weapon when parented. Set this in the Unity Inspector.</summary>
    [Tooltip("Local position applied to an equipped weapon when parented (used if no socket or to offset inside socket).")]
    [SerializeField] private Vector3 weaponLocalPosition = new Vector3(1.2f, 0.6f, 0.2f);
    
    /// <summary>Local Euler angles applied to an equipped weapon when parented. Set this in the Unity Inspector.</summary>
    [Tooltip("Local Euler angles applied to an equipped weapon when parented.")]
    [SerializeField] private Vector3 weaponLocalEuler = new Vector3(0f, 90f, 0f);
    
    /// <summary>Runtime instance of the weapon prefab, tracked for cleanup.</summary>
    private WeaponController _runtimeWeaponInstance;

    /// <summary>Automatically heal to max health on Start. Set this in the Unity Inspector.</summary>
    [Header("Events / Flags")]
    [SerializeField] private bool autoHealToMaxOnStart = false;

    /// <summary>Gets the player's current balance.</summary>
    public int Balance => balance;
    
    /// <summary>Gets the player's current health.</summary>
    public int CurrentHealth => currentHealth;
    
    /// <summary>Gets the player's maximum health.</summary>
    public int MaxHealth => maxHealth;
    
    /// <summary>Gets the player's assigned role.</summary>
    public PlayerRole Role => role;
    
    /// <summary>Gets whether the player is alive.</summary>
    public bool IsAlive => isAlive;
    
    /// <summary>Gets the player's vision range.</summary>
    public float VisionRange => visionRange;

    /// <summary>Gets the player's current role.</summary>
    public PlayerRole CurrentRole => role;
    
    /// <summary>Gets the player's money (alias for Balance).</summary>
    public int Money => balance;
    
    /// <summary>Gets the player's current HP (alias for CurrentHealth).</summary>
    public int CurrentHP => currentHealth;

    /// <summary>Gets the currently equipped <see cref="Weapon"/>.</summary>
    public Weapon EquippedWeapon => equippedWeapon;
    
    /// <summary>Gets a read-only list of owned <see cref="Weapon"/> definitions.</summary>
    public IReadOnlyList<Weapon> OwnedWeapons => ownedWeapons;
    
    /// <summary>Gets a read-only list of learned abilities.</summary>
    public IReadOnlyList<AbilityDefinition> LearnedAbilities => learnedAbilities;
    
    /// <summary>Gets the currently active ability.</summary>
    public AbilityDefinition ActiveAbility => _activeAbility;

    /// <summary>Initializes component references.</summary>
    void Awake()
    {
        if (roleAnnouncer == null)
        {
            roleAnnouncer = GetComponentInChildren<RoleAnnouncer>(true);
        }
        if (effectsController == null) effectsController = GetComponent<PlayerEffectsController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
    }

    /// <summary>Subscribes to global events.</summary>
    void OnEnable()
    {
        // Subscribe to global events (e.g., AchievementManager.NotifyEvent callbacks) if needed
    }

    /// <summary>Unsubscribes from events.</summary>
    void OnDisable()
    {
        // Unsubscribe from events
    }

    /// <summary>Initializes player state, assigns role from <see cref="GameManager"/>, and shows role via <see cref="RoleAnnouncer"/>.</summary>
    protected virtual void Start()
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

    /// <summary>Updates abilities, effects, weapon positioning, and handles attack input.</summary>
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

    /// <summary>
    /// Adds balance to the player.
    /// </summary>
    /// <param name="amount">Amount to add (must be positive).</param>
    public void AddBalance(int amount)
    {
        // 1. Validate amount > 0
        // 2. Add to balance
        // 3. Optionally notify UI / achievements
        if (amount <= 0) return;
        balance += amount;
    }

    /// <summary>
    /// Alias for <see cref="AddBalance"/> (for external calls).
    /// </summary>
    /// <param name="amount">Amount to add.</param>
    public void AddMoney(int amount)
    {
        // 1. Alias for AddBalance (for external calls)
        // 2. Could trigger money-specific events
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Attempts to spend balance.
    /// </summary>
    /// <param name="amount">Amount to spend.</param>
    /// <param name="allowNegative">Whether to allow negative balance.</param>
    /// <returns>True if transaction succeeded.</returns>
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

    /// <summary>
    /// Applies damage to the player. Calls <see cref="Die"/> if health reaches zero.
    /// </summary>
    /// <param name="dmg">Damage amount.</param>
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

    /// <summary>
    /// Handles player death.
    /// </summary>
    public void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        
        if (playerAnimator != null)
        {
            playerAnimator.TriggerDeath();
        }
        
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinCondition();
        }
        
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Heals the player by the specified amount (clamped to max health).
    /// </summary>
    /// <param name="hp">Health points to restore.</param>
    public void Heal(int hp)
    {
        // 1. Validate positive hp
        // 2. Add and clamp to maxHealth
        // 3. Play heal VFX if available
        if (hp <= 0) return;
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
    }

    /// <summary>
    /// Sets the player's role.
    /// </summary>
    /// <param name="newRole">The new <see cref="PlayerRole"/>.</param>
    public void SetRole(PlayerRole newRole)
    {
        // 1. Assign role
        // 2. Optionally update UI / achievements
        role = newRole;
    }

    /// <summary>
    /// Adds a <see cref="Weapon"/> to the player's inventory and auto-equips if none equipped.
    /// </summary>
    /// <param name="weapon">The weapon to acquire.</param>
    public void AcquireWeapon(Weapon weapon)
    {
        // 1. Null/duplicate check
        // 2. Add to ownedWeapons
        // 3. Auto-equip if none equipped
        if (weapon == null || ownedWeapons.Contains(weapon)) return;    
        ownedWeapons.Add(weapon);
        if (equippedWeapon == null) equippedWeapon = weapon;
    }

    /// <summary>
    /// Equips a <see cref="Weapon"/> from the player's inventory.
    /// </summary>
    /// <param name="weapon">The weapon to equip.</param>
    public void EquipWeapon(Weapon weapon)
    {
        // 1. Validate in ownedWeapons
        // 2. Assign equippedWeapon
        // 3. Update active WeaponController if present
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        equippedWeapon = weapon;
    }

    /// <summary>
    /// Learns a new ability if affordable.
    /// </summary>
    /// <param name="ability">The ability to learn.</param>
    /// <returns>True if the ability was successfully learned.</returns>
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

    /// <summary>
    /// Activates an ability. Not yet fully implemented.
    /// </summary>
    /// <param name="ability">The ability to activate.</param>
    /// <returns>True if activation succeeded.</returns>
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

    /// <summary>
    /// Activates an ability by its index in the learned abilities list.
    /// </summary>
    /// <param name="index">The index of the ability.</param>
    /// <returns>True if activation succeeded.</returns>
    public bool ActivateAbilityByIndex(int index)
    {
        // 1. Index bounds check
        // 2. Retrieve ability and call ActivateAbility
        if (index < 0 || index >= learnedAbilities.Count) return false;
        return ActivateAbility(learnedAbilities[index]);
    }

    /// <summary>
    /// Gets the remaining cooldown time for an ability.
    /// </summary>
    /// <param name="ability">The ability to check.</param>
    /// <returns>Remaining cooldown in seconds.</returns>
    public float GetCooldownRemaining(AbilityDefinition ability)
    {
        // 1. Look up dictionary; return clamped value
        if (ability == null) return 0f;
        return _cooldowns.TryGetValue(ability, out var t) ? Mathf.Max(0f, t) : 0f;
    }

    /// <summary>
    /// Checks if an ability is currently active.
    /// </summary>
    /// <param name="ability">The ability to check.</param>
    /// <returns>True if the ability is active.</returns>
    public bool IsAbilityActive(AbilityDefinition ability)
    {
        return _activeAbility == ability;
    }

    /// <summary>Ticks the active ability duration and ends it when expired.</summary>
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

    /// <summary>Ends the currently active ability.</summary>
    private void EndActiveAbility()
    {
        // 1. Deactivate effects
        // 2. Clear active ability
        // Basic cleanup for active ability
        _activeAbility = null;
        _activeAbilityTimeLeft = 0f;
    }

    /// <summary>Ticks down all ability cooldowns.</summary>
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

    /// <summary>
    /// Applies a speed effect. Not yet fully implemented.
    /// </summary>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="multiplier">Speed multiplier.</param>
    public void ApplySpeedEffect(float duration, float multiplier)
    {
        // 1. Forward to effectsController.ApplyEffect(EffectType.Speed,...)
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Applies an invisibility effect. Not yet fully implemented.
    /// </summary>
    /// <param name="duration">Duration in seconds.</param>
    public void ApplyInvisibilityEffect(float duration)
    {
        // 1. Forward to effectsController.ApplyEffect(EffectType.Invisibility,...)
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Sets the current <see cref="WeaponController"/> and initializes it.
    /// </summary>
    /// <param name="weapon">The weapon controller to set.</param>
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

    /// <summary>Performs an attack using the current <see cref="WeaponController"/>.</summary>
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

    /// <summary>
    /// Uses an item from the hotbar. Not yet fully implemented.
    /// </summary>
    /// <param name="item">The item to use.</param>
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
