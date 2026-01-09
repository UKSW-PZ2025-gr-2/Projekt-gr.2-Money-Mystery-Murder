using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bot player that inherits from <see cref="Player"/> but does not announce its role.
/// Used for AI-controlled players in the game.
/// </summary>
public class Bot : Player
{
    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float changeDirectionTime = 2f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField, Tooltip("Which layers the bot will consider as attack targets (set to Player layer in Inspector)")]
    private LayerMask targetLayers = ~0;

    [Header("Weapon")]
    [SerializeField] private List<WeaponData> possibleWeapons = new List<WeaponData>();
    [SerializeField, Tooltip("Fallback weapon if possibleWeapons list is empty")]
    private WeaponData knifeWeapon;

    private WeaponData _equippedWeapon;
    private Vector3 _moveDirection;
    private float _nextChangeTime;
    private float _nextAttackTime;
    private PlayerMovement _movement;
    private PlayerAnimator _playerAnimator;

    /// <summary>
    /// Initializes bot state, assigns role from <see cref="RoleManager"/>.
    /// Overrides <see cref="Player.Start"/> to skip role announcement.
    /// </summary>
    protected override void Start()
    {
        base.Start(); // Call base Start first

        _movement = GetComponent<PlayerMovement>();
        if (_movement != null)
        {
            _movement.enabled = false; // Disable player input movement
        }

        _playerAnimator = GetComponent<PlayerAnimator>();
        if (_playerAnimator == null)
        {
            Debug.LogWarning($"[Bot] {gameObject.name} has no PlayerAnimator component for walk animation.");
        }

        // Select and equip a random weapon
        _equippedWeapon = SelectRandomWeapon();
        if (_equippedWeapon != null)
        {
            AcquireWeapon(_equippedWeapon);
            EquipWeapon(_equippedWeapon);
            attackRange = _equippedWeapon.range; // Set attack range to match weapon
            Debug.Log($"[Bot] {gameObject.name} equipped {_equippedWeapon.displayName} with range {attackRange}");
        }
        else
        {
            Debug.LogWarning($"[Bot] {gameObject.name} has no weapon to equip (possibleWeapons and knifeWeapon are null)");
        }

        _moveDirection = Random.insideUnitCircle.normalized;
        _nextChangeTime = Time.time + changeDirectionTime;
        _nextAttackTime = Time.time + attackCooldown;
    }

    /// <summary>
    /// Selects a random weapon from the possibleWeapons list.
    /// Falls back to knifeWeapon if the list is empty.
    /// </summary>
    private WeaponData SelectRandomWeapon()
    {
        if (possibleWeapons != null && possibleWeapons.Count > 0)
        {
            // Filter out null entries
            List<WeaponData> validWeapons = new List<WeaponData>();
            foreach (WeaponData weapon in possibleWeapons)
            {
                if (weapon != null)
                {
                    validWeapons.Add(weapon);
                }
            }

            if (validWeapons.Count > 0)
            {
                int randomIndex = Random.Range(0, validWeapons.Count);
                return validWeapons[randomIndex];
            }
        }

        // Fallback to knife weapon
        return knifeWeapon;
    }

    private void Update()
    {
        if (!IsAlive) 
        {
            // Stop all movement and animation updates when dead
            if (_playerAnimator != null)
            {
                _playerAnimator.SetMovementState(false);
            }
            return;
        }
        
        // Random movement
        if (Time.time > _nextChangeTime)
        {
            _moveDirection = Random.insideUnitCircle.normalized;
            _nextChangeTime = Time.time + changeDirectionTime;
        }

        // Move and update walk animation
        bool isMoving = _moveDirection.sqrMagnitude > 0.01f;
        if (isMoving)
        {
            transform.Translate(_moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }

        // Update walk animation state
        if (_playerAnimator != null)
        {
            _playerAnimator.SetMovementState(isMoving);
        }

        // Attack during night
        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Night)
        {
            AttackNearbyPlayers();
        }
    }

    private void AttackNearbyPlayers()
    {
        if (Time.time < _nextAttackTime) return;

        // Find the closest player within attack range
        Player closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayers);

        foreach (Collider2D hit in hits)
        {
            Player player = null;
            if (hit != null)
            {
                player = hit.GetComponent<Player>();
                if (player == null) player = hit.GetComponentInParent<Player>();
                if (player == null) player = hit.GetComponentInChildren<Player>();
            }

            if (player != null && player != this && player.IsAlive)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        if (closestPlayer != null)
        {
            // Face the player
            Vector3 direction = (closestPlayer.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Attack: prefer direct damage (_equippedWeapon) but log details and fall back to WeaponSystem if needed
            Debug.Log($"[Bot] {gameObject.name} attacking {closestPlayer.gameObject.name} with _equippedWeapon={(_equippedWeapon != null ? _equippedWeapon.displayName : "null")} weaponSystemWeapon={(weaponSystem != null && weaponSystem.CurrentWeapon != null ? weaponSystem.CurrentWeapon.displayName : "null")}");

            int dmg = _equippedWeapon != null ? _equippedWeapon.damage : (weaponSystem?.CurrentWeapon?.damage ?? 0);

            if (dmg > 0)
            {
                Debug.Log($"[Bot] {gameObject.name} will deal {dmg} damage to {closestPlayer.gameObject.name}");

                // Play weapon visuals (weapon animator + SFX) if weaponSystem is present
                if (weaponSystem != null)
                {
                    try { weaponSystem.PlayAttackVisuals(); } catch (System.Exception e) { Debug.LogWarning($"[Bot] {gameObject.name} PlayAttackVisuals failed: {e.Message}"); }
                }

                // Trigger own attack animation if available (player body animator)
                var selfAnim = GetComponent<PlayerAnimator>();
                if (selfAnim != null)
                {
                    try { selfAnim.TriggerAttack(); } catch (System.Exception e) { Debug.LogWarning($"[Bot] {gameObject.name} failed to TriggerAttack: {e.Message}"); }
                }
                else
                {
                    Debug.LogWarning($"[Bot] {gameObject.name} has no PlayerAnimator component to play attack animation.");
                }

                // Apply damage
                closestPlayer.TakeDamage(dmg);
                Debug.Log($"[Bot] {gameObject.name} dealt {dmg} damage to {closestPlayer.gameObject.name}");

                // Trigger hit animation on the target if available
                var targetAnim = closestPlayer.GetComponent<PlayerAnimator>();
                if (targetAnim != null)
                {
                    try { targetAnim.TriggerHit(); } catch { }
                }
            }
            else
            {
                Debug.Log($"[Bot] {gameObject.name} computed dmg=0, falling back to PerformAttack()");
                PerformAttack();
            }

            _nextAttackTime = Time.time + attackCooldown;
        }
    }
}
