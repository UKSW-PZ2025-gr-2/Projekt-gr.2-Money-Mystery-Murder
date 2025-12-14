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
    None,
    Civilian,
    Detective,
    Murderer
}

/// <summary>
/// Core player controller managing health, balance, inventory, abilities, and combat.
/// </summary>
public class Player : MonoBehaviour
{
    #region Serialized Fields
    
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
    [SerializeField] private List<ShopItem> hotbarItems = new();

    [Header("Abilities")]
    [SerializeField] private List<AbilityDefinition> learnedAbilities = new();

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

    [Header("Events / Flags")]
    [SerializeField] private bool autoHealToMaxOnStart = false;
    
    #endregion

    #region Private Fields
    
    private readonly Dictionary<AbilityDefinition, float> _cooldowns = new();
    private AbilityDefinition _activeAbility;
    private float _activeAbilityTimeLeft;
    private WeaponController _runtimeWeaponInstance;
    
    #endregion

    #region Public Properties
    
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
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        // Subscribe to global events if needed
    }

    private void OnDisable()
    {
        // Unsubscribe from events
    }

    protected virtual void Start()
    {
        InitializePlayer();
    }

    private void Update()
    {
        UpdateAbilities();
        UpdateEffects();
        UpdateWeaponTransform();
        HandleInput();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        if (roleAnnouncer == null)
        {
            roleAnnouncer = GetComponentInChildren<RoleAnnouncer>(true);
        }
        
        if (effectsController == null)
        {
            effectsController = GetComponent<PlayerEffectsController>();
        }
        
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }
    }

    private void InitializePlayer()
    {
        if (autoHealToMaxOnStart)
        {
            currentHealth = maxHealth;
        }

        AssignRoleIfNeeded();
        ShowRole();
        InitializeWeapon();
    }

    private void AssignRoleIfNeeded()
    {
        if (role == PlayerRole.None && 
            GameManager.Instance != null && 
            GameManager.Instance.RoleManager != null)
        {
            role = GameManager.Instance.RoleManager.PickRandomRoleFromPool();
        }
    }

    private void ShowRole()
    {
        if (roleAnnouncer != null)
        {
            roleAnnouncer.ShowRole(role);
        }
    }

    private void InitializeWeapon()
    {
        if (currentWeapon != null)
        {
            SetCurrentWeapon(currentWeapon);
        }
    }
    
    #endregion

    #region Update Methods
    
    private void UpdateAbilities()
    {
        TickActiveAbility();
        TickCooldowns();
    }

    private void UpdateEffects()
    {
        if (effectsController != null)
        {
            effectsController.UpdateEffects();
        }
    }

    private void UpdateWeaponTransform()
    {
        if (currentWeapon == null) return;

        GameObject weaponGO = currentWeapon.gameObject;
        Transform parent = weaponSocket != null ? weaponSocket : transform;

        if (weaponGO.transform.parent != parent)
        {
            weaponGO.transform.SetParent(parent, false);
        }

        weaponGO.transform.localPosition = weaponLocalPosition;
        weaponGO.transform.localRotation = Quaternion.Euler(weaponLocalEuler);
    }

    private void HandleInput()
    {
        HandleAbilityInput();
        HandleAttackInput();
    }
    
    #endregion

    #region Input Handling
    
    private void HandleAbilityInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) ActivateAbilityByIndex(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) ActivateAbilityByIndex(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) ActivateAbilityByIndex(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) ActivateAbilityByIndex(3);
        else if (keyboard.digit5Key.wasPressedThisFrame) ActivateAbilityByIndex(4);
        else if (keyboard.digit6Key.wasPressedThisFrame) ActivateAbilityByIndex(5);
        else if (keyboard.digit7Key.wasPressedThisFrame) ActivateAbilityByIndex(6);
        else if (keyboard.digit8Key.wasPressedThisFrame) ActivateAbilityByIndex(7);
        else if (keyboard.digit9Key.wasPressedThisFrame) ActivateAbilityByIndex(8);
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateAbilityByIndex(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateAbilityByIndex(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ActivateAbilityByIndex(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) ActivateAbilityByIndex(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) ActivateAbilityByIndex(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) ActivateAbilityByIndex(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) ActivateAbilityByIndex(6);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) ActivateAbilityByIndex(7);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) ActivateAbilityByIndex(8);
#endif
    }

    private void HandleAttackInput()
    {
#if ENABLE_INPUT_SYSTEM
        bool pressed = Mouse.current != null 
            ? Mouse.current.leftButton.wasPressedThisFrame 
            : Input.GetMouseButtonDown(0);
#else
        bool pressed = Input.GetMouseButtonDown(0);
#endif

        if (pressed)
        {
            PerformAttack();
        }
    }
    
    #endregion

    #region Economy
    
    public void AddBalance(int amount)
    {
        if (amount <= 0) return;
        balance += amount;
    }

    public void AddMoney(int amount)
    {
        AddBalance(amount);
    }

    public bool SpendBalance(int amount, bool allowNegative = false)
    {
        if (amount <= 0) return true;
        if (!allowNegative && balance < amount) return false;
        
        balance -= amount;
        return true;
    }
    
    #endregion

    #region Health & Combat
    
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        
        currentHealth -= dmg;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        
        TriggerDeathAnimation();
        DisableMovement();
        NotifyGameManager();
        
        gameObject.SetActive(false);
    }

    public void Heal(int hp)
    {
        if (hp <= 0) return;
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
    }

    private void TriggerDeathAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.TriggerDeath();
        }
    }

    private void DisableMovement()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }

    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinCondition();
        }
    }
    
    #endregion

    #region Role Management
    
    public void SetRole(PlayerRole newRole)
    {
        role = newRole;
    }
    
    #endregion

    #region Weapon Management
    
    public void AcquireWeapon(Weapon weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon)) return;
        
        ownedWeapons.Add(weapon);
        
        if (equippedWeapon == null)
        {
            equippedWeapon = weapon;
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        equippedWeapon = weapon;
    }

    public void SetCurrentWeapon(WeaponController weapon)
    {
        CleanupRuntimeWeapon();

        if (weapon == null)
        {
            currentWeapon = null;
            return;
        }

        WeaponController instance = InstantiateWeapon(weapon);
        if (instance == null) return;

        currentWeapon = instance;
        currentWeapon.Initialize(this);
        ParentWeapon(currentWeapon);
    }

    public void PerformAttack()
    {
        if (currentWeapon == null) return;

        currentWeapon.Attack();
        TriggerAttackAnimation();
    }

    private void CleanupRuntimeWeapon()
    {
        if (_runtimeWeaponInstance != null)
        {
            try
            {
                Destroy(_runtimeWeaponInstance.gameObject);
            }
            catch
            {
                // Ignore cleanup errors
            }
            _runtimeWeaponInstance = null;
        }
    }

    private WeaponController InstantiateWeapon(WeaponController weapon)
    {
        GameObject instantiatedGO = Instantiate(weapon.gameObject);
        WeaponController instance = instantiatedGO.GetComponent<WeaponController>();
        
        if (instance == null)
        {
            Debug.LogError("SetCurrentWeapon: instantiated weapon GameObject has no WeaponController component");
            Destroy(instantiatedGO);
            return null;
        }

        _runtimeWeaponInstance = instance;
        return instance;
    }

    private void ParentWeapon(WeaponController weapon)
    {
        try
        {
            GameObject go = weapon.gameObject;
            Transform parent = weaponSocket != null ? weaponSocket : transform;
            
            go.transform.SetParent(parent, false);
            go.transform.localPosition = weaponLocalPosition;
            go.transform.localRotation = Quaternion.Euler(weaponLocalEuler);
        }
        catch (System.Exception)
        {
            // Ignore parenting errors
        }
    }

    private void TriggerAttackAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttack();
        }
    }
    
    #endregion

    #region Ability System
    
    public bool LearnAbility(AbilityDefinition ability)
    {
        if (!CanLearnAbility(ability)) return false;
        if (!SpendBalance(ability.cost)) return false;
        
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f;
        return true;
    }

    public bool ActivateAbility(AbilityDefinition ability)
    {
        if (!CanActivateAbility(ability)) return false;
        
        EndActiveAbility();
        StartAbility(ability);
        ApplyAbilityEffect(ability);
        StartCooldown(ability);
        
        return true;
    }

    public bool ActivateAbilityByIndex(int index)
    {
        if (index < 0 || index >= learnedAbilities.Count) return false;
        return ActivateAbility(learnedAbilities[index]);
    }

    public float GetCooldownRemaining(AbilityDefinition ability)
    {
        if (ability == null) return 0f;
        return _cooldowns.TryGetValue(ability, out float t) ? Mathf.Max(0f, t) : 0f;
    }

    public bool IsAbilityActive(AbilityDefinition ability)
    {
        return _activeAbility == ability;
    }

    private bool CanLearnAbility(AbilityDefinition ability)
    {
        return ability != null && !learnedAbilities.Contains(ability);
    }

    private bool CanActivateAbility(AbilityDefinition ability)
    {
        if (ability == null || !learnedAbilities.Contains(ability)) return false;
        if (GetCooldownRemaining(ability) > 0f) return false;
        return true;
    }

    private void StartAbility(AbilityDefinition ability)
    {
        _activeAbility = ability;
        _activeAbilityTimeLeft = ability.duration;
    }

    private void ApplyAbilityEffect(AbilityDefinition ability)
    {
        switch (ability.kind)
        {
            case AbilityKind.Invisibility:
                ApplyInvisibilityEffect(ability.duration);
                break;
            
            case AbilityKind.SpeedBoost:
                ApplySpeedEffect(ability.duration, ability.magnitude);
                break;
            
            case AbilityKind.Heal:
                Heal((int)ability.magnitude);
                break;
        }
    }

    private void StartCooldown(AbilityDefinition ability)
    {
        _cooldowns[ability] = ability.cooldown;
    }

    private void TickActiveAbility()
    {
        if (_activeAbility == null) return;

        _activeAbilityTimeLeft -= Time.deltaTime;
        
        if (_activeAbilityTimeLeft <= 0f)
        {
            EndActiveAbility();
        }
    }

    private void EndActiveAbility()
    {
        _activeAbility = null;
        _activeAbilityTimeLeft = 0f;
    }

    private void TickCooldowns()
    {
        if (_cooldowns.Count == 0) return;

        List<AbilityDefinition> keys = new List<AbilityDefinition>(_cooldowns.Keys);
        foreach (AbilityDefinition key in keys)
        {
            if (_cooldowns[key] > 0f)
            {
                _cooldowns[key] = Mathf.Max(0f, _cooldowns[key] - Time.deltaTime);
            }
        }
    }

    public void ApplySpeedEffect(float duration, float multiplier)
    {
        if (effectsController != null)
        {
            effectsController.ApplyEffect(EffectType.Speed, duration, multiplier);
        }
    }

    public void ApplyInvisibilityEffect(float duration)
    {
        if (effectsController != null)
        {
            effectsController.ApplyEffect(EffectType.Invisibility, duration, 1f);
        }
    }
    
    #endregion

    #region Item Usage
    
    public void UseItem(ShopItem item)
    {
        throw new System.NotImplementedException();
    }
    
    #endregion
}
