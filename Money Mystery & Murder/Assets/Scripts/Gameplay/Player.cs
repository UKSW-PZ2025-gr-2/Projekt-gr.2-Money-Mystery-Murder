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

    [Header("Debug View")] public bool autoHealToMaxOnStart = false;

    public int Balance => balance;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public PlayerRole Role => role;
    public WeaponDefinition EquippedWeapon => equippedWeapon;
    public IReadOnlyList<WeaponDefinition> OwnedWeapons => ownedWeapons;
    public IReadOnlyList<AbilityDefinition> LearnedAbilities => learnedAbilities;
    public AbilityDefinition ActiveAbility => _activeAbility;

    void Start()
    {
        if (autoHealToMaxOnStart) currentHealth = maxHealth;
    }

    void Update()
    {
        TickActiveAbility();
        TickCooldowns();
    }

    // ----- Currency (saldo) -----
    public void AddBalance(int amount)
    {
        if (amount <= 0) return;
        balance += amount;
    }

    public bool SpendBalance(int amount)
    {
        if (amount <= 0) return true;
        if (balance < amount) return false;
        balance -= amount;
        return true;
    }

    // ----- Health (¿ycie) -----
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // TODO: death handling
        }
    }

    public void Heal(int hp)
    {
        if (hp <= 0) return;
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
    }

    // ----- Role (rola) -----
    public void SetRole(PlayerRole newRole) => role = newRole;

    // ----- Weapons (przedmioty) -----
    public void AcquireWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon)) return;
        ownedWeapons.Add(weapon);
        if (equippedWeapon == null) equippedWeapon = weapon;
    }

    public void EquipWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || !ownedWeapons.Contains(weapon)) return;
        equippedWeapon = weapon;
    }

    // ----- Abilities API -----
    public bool LearnAbility(AbilityDefinition ability)
    {
        if (ability == null || learnedAbilities.Contains(ability)) return false;
        if (!SpendBalance(ability.cost)) return false;
        learnedAbilities.Add(ability);
        _cooldowns[ability] = 0f; // ready to use
        return true;
    }

    public bool ActivateAbility(AbilityDefinition ability)
    {
        if (ability == null) return false;
        if (!learnedAbilities.Contains(ability)) return false;
        if (!_cooldowns.ContainsKey(ability)) _cooldowns[ability] = 0f;
        if (_cooldowns[ability] > 0f) return false; // still cooling down

        if (_activeAbility != null)
        {
            EndActiveAbility();
        }

        _activeAbility = ability;
        _activeAbilityTimeLeft = ability.duration;
        _cooldowns[ability] = ability.cooldown; // start cooldown now

        if (ability.allowWallPass && ability.wallPassLayer >= 0)
        {
            _previousLayer = gameObject.layer;
            gameObject.layer = ability.wallPassLayer;
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
        _activeAbilityTimeLeft -= Time.deltaTime;
        if (_activeAbilityTimeLeft <= 0f)
        {
            EndActiveAbility();
        }
    }

    private void EndActiveAbility()
    {
        if (_activeAbility == null) return;
        if (_activeAbility.allowWallPass && _activeAbility.wallPassLayer >= 0)
        {
            // revert to previous layer if known
            gameObject.layer = _previousLayer >= 0 ? _previousLayer : 0;
        }
        _activeAbility = null;
        _previousLayer = -1;
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
                if (_cooldowns[ability] < 0f) _cooldowns[ability] = 0f;
            }
        }
    }

    public float GetSpeedMultiplier()
    {
        if (_activeAbility == null) return 1f;
        return _activeAbility.speedMultiplier;
    }
}
