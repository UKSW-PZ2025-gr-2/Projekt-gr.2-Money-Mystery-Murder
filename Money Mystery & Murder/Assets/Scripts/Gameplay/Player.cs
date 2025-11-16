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

    [Header("Inventory / Progression")]
    [SerializeField] private WeaponDefinition equippedWeapon; // equipped weapon
    [SerializeField] private List<WeaponDefinition> ownedWeapons = new(); // all owned weapons

    [Header("Abilities")]
    [SerializeField] private List<AbilityDefinition> learnedAbilities = new(); // all learned abilities
    private readonly Dictionary<AbilityDefinition, float> _cooldowns = new(); // time left until can use again
    private AbilityDefinition _activeAbility; // currently running timed ability
    private float _activeAbilityTimeLeft;

    [Header("UI")]
    [SerializeField] private RoleAnnouncer roleAnnouncer;

    [Header("Debug View")] public bool autoHealToMaxOnStart = false;

    public int Balance => balance;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public PlayerRole Role => role;
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
    }

    void Start()
    {
        if (autoHealToMaxOnStart) currentHealth = maxHealth;

        // If role not preassigned, pick randomly from pool for local testing
        if (role == PlayerRole.None && GameManager.Instance != null)
        {
            role = GameManager.Instance.PickRandomRoleFromPool();
            Debug.Log($"[Player] Random role assigned to {name}: {role}");
        }
        else
        {
            Debug.Log($"[Player] Existing role for {name}: {role}");
        }

        // Announce role at start
        if (roleAnnouncer != null)
        {
            Debug.Log($"[Player] Announcing role {role} for {name}");
            roleAnnouncer.ShowRole(role);
        }
    }

    void Update()
    {
        TickActiveAbility();
        TickCooldowns();
    }

    // ----- Currency -----
    public void AddBalance(int amount)
    {
        if (amount <= 0) return;
        balance += amount;
        Debug.Log($"[Player] {name} balance increased by {amount}. New balance: {balance}");
    }

    public bool SpendBalance(int amount)
    {
        if (amount <= 0) return true;
        if (balance < amount)
        {
            Debug.Log($"[Player] {name} cannot spend {amount}. Current balance: {balance}");
            return false;
        }
        balance -= amount;
        Debug.Log($"[Player] {name} spent {amount}. New balance: {balance}");
        return true;
    }

    // ----- Health -----
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHealth -= dmg;
        Debug.Log($"[Player] {name} took {dmg} damage. HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"[Player] {name} died.");
            // TODO: death handling
        }
    }

    public void Heal(int hp)
    {
        if (hp <= 0) return;
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
        Debug.Log($"[Player] {name} healed {hp}. HP: {currentHealth}/{maxHealth}");
    }

    // ----- Role -----
    public void SetRole(PlayerRole newRole)
    {
        role = newRole;
        Debug.Log($"[Player] Role set manually for {name}: {role}");
    }

    // ----- Weapons -----
    public void AcquireWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon)) return;    
        ownedWeapons.Add(weapon);
        Debug.Log($"[Player] {name} acquired weapon {weapon.name}");
        if (equippedWeapon == null)
        {
            equippedWeapon = weapon;
            Debug.Log($"[Player] {name} auto-equipped weapon {weapon.name}");
        }
    }

    public void EquipWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        equippedWeapon = weapon;
        Debug.Log($"[Player] {name} equipped weapon {weapon.name}");
    }

    // ----- Abilities API -----
    public bool LearnAbility(AbilityDefinition ability)
    {
        if (ability == null || learnedAbilities.Contains(ability)) return false;
        if (!SpendBalance(ability.cost)) return false;
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f; // ready to use
        Debug.Log($"[Player] {name} learned ability {ability.displayName}");
        return true;
    }

    public bool ActivateAbility(AbilityDefinition ability)
    {
        if (ability == null) return false;
        if (!learnedAbilities.Contains(ability)) return false;
        if (!_cooldowns.ContainsKey(ability)) _cooldowns[ability] = 0f;
        if (_cooldowns[ability] > 0f)
        {
            Debug.Log($"[Player] {name} tried to activate {ability.displayName} but it's on cooldown: {_cooldowns[ability]:F2}s left");
            return false; // still cooling down
        }

        if (_activeAbility != null)
        {
            Debug.Log($"[Player] {name} ending active ability {_activeAbility.displayName} early to start {ability.displayName}");
            EndActiveAbility();
        }

        _activeAbility = ability;
        _activeAbilityTimeLeft = ability.duration;
        _cooldowns[ability] = ability.cooldown; // start cooldown now
        Debug.Log($"[Player] {name} activated ability {ability.displayName} (duration: {ability.duration}s, cooldown: {ability.cooldown}s)");

        // Apply effects' activation hooks
        if (_activeAbility.effects != null)
        {
            foreach (var effect in _activeAbility.effects)
            {
                if (effect != null)
                {
                    Debug.Log($"[Player] {name} activating effect {effect.name} for ability {ability.displayName}");
                    effect.OnActivate(this);
                }
            }
        }
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
        return _cooldowns.TryGetValue(ability, out var t) ? Mathf.Max(0f, t) : 0f;
    }

    public bool IsAbilityActive(AbilityDefinition ability)
    {
        return _activeAbility == ability;
    }

    private void TickActiveAbility()
    {
        if (_activeAbility == null) return;
        if (_activeAbility.duration <= 0f)
        {
            EndActiveAbility();
            return;
        }
        // Tick effects
        if (_activeAbility.effects != null)
        {
            foreach (var effect in _activeAbility.effects)
            {
                if (effect != null)
                {
                    effect.OnTick(this, Time.deltaTime);
                }
            }
        }

        _activeAbilityTimeLeft -= Time.deltaTime;
        if (_activeAbilityTimeLeft <= 0f)
        {
            Debug.Log($"[Player] {name} ability {_activeAbility.displayName} ended (duration elapsed)");
            EndActiveAbility();
        }
    }

    private void EndActiveAbility()
    {
        if (_activeAbility == null) return;

        // Call deactivate on effects
        if (_activeAbility.effects != null)
        {
            foreach (var effect in _activeAbility.effects)
            {
                if (effect != null)
                {
                    Debug.Log($"[Player] {name} deactivating effect {effect.name} from ability {_activeAbility.displayName}");
                    effect.OnDeactivate(this);
                }
            }
        }

        Debug.Log($"[Player] {name} ability {_activeAbility.displayName} fully deactivated");
        _activeAbility = null;
    }

    private void TickCooldowns()
    {
        if (_cooldowns.Count == 0) return;
        var keys = new List<AbilityDefinition>(_cooldowns.Keys);
        foreach (var ability in keys)
        {
            if (_cooldowns[ability] > 0f)
            {
                _cooldowns[ability] -= Time.deltaTime;
                if (_cooldowns[ability] < 0f)
                {
                    _cooldowns[ability] = 0f;
                    Debug.Log($"[Player] {name} cooldown finished for ability {ability.displayName}");
                }
            }
        }
    }

}
