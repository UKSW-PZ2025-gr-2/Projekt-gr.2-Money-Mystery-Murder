using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private List<WeaponData> ownedWeapons = new();

    [Header("Abilities")]
    [SerializeField] private List<Ability> learnedAbilities = new();

    [Header("UI")]
    [SerializeField] private RoleAnnouncer roleAnnouncer;

    [Header("Effects & Animation")]
    [SerializeField] private PlayerEffectsController effectsController;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Weapon System")]
    [SerializeField] protected WeaponSystem weaponSystem;
    
    [Header("Night Vision")]
    [SerializeField] private NightVisionController nightVisionController;

    [Header("Events / Flags")]
    [SerializeField] private bool autoHealToMaxOnStart = false;
    
    #endregion

    #region Private Fields
    
    private readonly Dictionary<Ability, float> _cooldowns = new();
    private Ability _activeAbility;
    private float _activeAbilityTimeLeft;
    
    #endregion

    #region Public Properties
    
    public int Balance => balance;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public PlayerRole Role => role;
    public bool IsAlive => isAlive;
    public float VisionRange => visionRange;
    public WeaponData EquippedWeapon => weaponSystem?.CurrentWeapon;
    public IReadOnlyList<WeaponData> OwnedWeapons => ownedWeapons;
    public IReadOnlyList<Ability> LearnedAbilities => learnedAbilities;
    public Ability ActiveAbility => _activeAbility;
    
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
        
        if (nightVisionController == null)
        {
            nightVisionController = GetComponent<NightVisionController>();
            
            // Only add for actual player, not bots
            bool isBot = gameObject.name.Contains("BOT");
            
            if (nightVisionController == null && !isBot)
            {
                // Auto-add if missing (only for player)
                nightVisionController = gameObject.AddComponent<NightVisionController>();
                Debug.Log($"[Player] Auto-added NightVisionController to player {gameObject.name}");
            }
            else if (isBot && nightVisionController != null)
            {
                // Remove from bots if exists
                Destroy(nightVisionController);
                nightVisionController = null;
                Debug.Log($"[Player] Removed NightVisionController from bot {gameObject.name}");
            }
        }
        
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
            
            if (playerAnimator == null)
            {
                // Try to find it in children
                playerAnimator = GetComponentInChildren<PlayerAnimator>();
            }
            
            if (playerAnimator == null)
            {
                Debug.LogWarning($"[Player] PlayerAnimator component not found on {gameObject.name}. Attack animations will not play. Please add a PlayerAnimator component.");
            }
        }
        
        if (weaponSystem == null)
        {
            weaponSystem = GetComponentInChildren<WeaponSystem>();
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
        if (weaponSystem != null)
        {
            weaponSystem.Initialize(this);
            
            // Equip first weapon from owned weapons list if available
            if (ownedWeapons.Count > 0 && ownedWeapons[0] != null)
            {
                weaponSystem.EquipWeapon(ownedWeapons[0]);
            }
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

    private void HandleInput()
    {
        HandleAbilityInput();
        HandleAttackInput();
    }
    
    #endregion

    #region Input Handling
    
    private void HandleAbilityInput()
    {
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
    }

    private void HandleAttackInput()
    {
        bool pressed = false;
        
        // Default to left mouse button (Space key is used for other things)
        if (Mouse.current != null)
        {
            pressed = Mouse.current.leftButton.wasPressedThisFrame;
        }

        if (pressed)
        {
            Debug.Log("[Player] Attack input detected.");
            PerformAttack();
        }
    }
    
    #endregion

    #region Economy
    
    public void AddBalance(int amount)
    {
        if (amount <= 0) return;
        
        // Play money sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMoney();
        
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
        
        // Play pain sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPain();
        
        Debug.Log($"[Player] {gameObject.name} took {dmg} damage, HP: {currentHealth} -> {currentHealth - dmg}");
        
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
    
    public void AcquireWeapon(WeaponData weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon)) return;
        
        ownedWeapons.Add(weapon);
        
        // If this is the first weapon, equip it automatically
        if (ownedWeapons.Count == 1 && weaponSystem != null)
        {
            weaponSystem.EquipWeapon(weapon);
        }
    }

    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        
        if (weaponSystem != null)
        {
            weaponSystem.EquipWeapon(weapon);
        }
    }

    public void PerformAttack()
    {
        if (weaponSystem == null)
        {
            Debug.LogWarning($"[Player] Cannot attack - weaponSystem is null on {gameObject.name}");
            return;
        }
        
        if (IsInMinigameOrShop())
        {
            Debug.Log($"[Player] Cannot attack - {gameObject.name} is in minigame or shop");
            return;
        }

        Debug.Log($"[Player] {gameObject.name} performing attack");
        
        // Trigger player animation first
        TriggerAttackAnimation();
        
        // Then perform the actual weapon attack
        weaponSystem.Attack();
    }

    private void TriggerAttackAnimation()
    {
        if (playerAnimator == null)
        {
            Debug.LogWarning($"[Player] Cannot trigger attack animation on {gameObject.name} - playerAnimator is null. Make sure PlayerAnimator component is attached.");
            return;
        }
        
        Debug.Log($"[Player] Triggering attack animation for {gameObject.name}");
        playerAnimator.TriggerAttack();
    }

    /// <summary>
    /// Check if the player is currently in a minigame or shop (cannot attack).
    /// </summary>
    public bool IsInMinigameOrShop()
    {
        // Check if player is in a minigame
        var minigameActivators = Object.FindObjectsByType<MinigameActivator>(FindObjectsSortMode.None);
        foreach (var activator in minigameActivators)
        {
            if (activator.CurrentMinigame != null && 
                activator.CurrentMinigame.IsRunning && 
                activator.CurrentMinigame.ActivatingPlayer == this)
            {
                return true;
            }
        }

        // Check if player is in a shop
        var shopUIs = Object.FindObjectsByType<ShopUI>(FindObjectsSortMode.None);
        foreach (var shopUI in shopUIs)
        {
            if (shopUI.IsOpen)
            {
                // Shop UI doesn't expose the current player, but if it's open and this player's movement is disabled by a shop, they're in it
                // We'll check by seeing if any shop has disabled our movement
                var movement = GetComponent<PlayerMovement>();
                if (movement != null && !movement.enabled)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    #endregion

    #region Ability System
    
    public bool AcquireAbility(Ability ability)
    {
        if (ability == null || learnedAbilities.Contains(ability)) return false;
        
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f;
        return true;
    }
    
    public bool LearnAbility(Ability ability)
    {
        if (!CanLearnAbility(ability)) return false;
        if (!SpendBalance(ability.cost)) return false;
        
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f;
        return true;
    }

    public bool ActivateAbility(Ability ability)
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

    public float GetCooldownRemaining(Ability ability)
    {
        if (ability == null) return 0f;
        return _cooldowns.TryGetValue(ability, out float t) ? Mathf.Max(0f, t) : 0f;
    }

    public bool IsAbilityActive(Ability ability)
    {
        return _activeAbility == ability;
    }

    private bool CanLearnAbility(Ability ability)
    {
        return ability != null && !learnedAbilities.Contains(ability);
    }

    private bool CanActivateAbility(Ability ability)
    {
        if (ability == null || !learnedAbilities.Contains(ability)) return false;
        if (GetCooldownRemaining(ability) > 0f) return false;
        return true;
    }

    private void StartAbility(Ability ability)
    {
        _activeAbility = ability;
        _activeAbilityTimeLeft = ability.duration;
    }

    private void ApplyAbilityEffect(Ability ability)
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

    private void StartCooldown(Ability ability)
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

        List<Ability> keys = new List<Ability>(_cooldowns.Keys);
        foreach (Ability key in keys)
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
