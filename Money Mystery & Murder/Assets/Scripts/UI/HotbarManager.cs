using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the player's hotbar for quick access to weapons and abilities.
/// Displays when toggle key is pressed, allows using items with number keys 1-9.
/// Automatically adds purchased items to available slots.
/// </summary>
public class HotbarManager : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// The main panel GameObject that contains the hotbar UI elements.
    /// </summary>
    [SerializeField] private GameObject hotbarPanel;
    
    /// <summary>
    /// Array of hotbar slots that can hold weapons or abilities. Default size is 9 slots.
    /// </summary>
    [SerializeField] private HotbarSlot[] hotbarSlots = new HotbarSlot[9];
    
    /// <summary>
    /// The canvas component used for rendering the hotbar in world space.
    /// </summary>
    [SerializeField] private Canvas hotbarCanvas;
    
    [Header("Settings")]
    /// <summary>
    /// Maximum number of slots available in the hotbar.
    /// </summary>
    [SerializeField] private int maxSlots = 9;
    
    /// <summary>
    /// Distance from the player to display the hotbar in world space.
    /// </summary>
    [SerializeField] private float worldSpaceDistance = 2f;
    
    /// <summary>
    /// Offset position relative to the player's transform for the hotbar.
    /// </summary>
    [SerializeField] private Vector3 worldSpaceOffset = new Vector3(0, -1.5f, 0);
    
    /// <summary>
    /// Whether the hotbar is visible at start.
    /// </summary>
    [SerializeField] private bool startVisible = true;
    
    /// <summary>
    /// Reference to the player who owns this hotbar.
    /// </summary>
    private Player player;
    
    /// <summary>
    /// Tracks whether the hotbar UI is currently visible.
    /// </summary>
    private bool isHotbarVisible = false;
    
    /// <summary>
    /// Dictionary tracking remaining cooldown times for abilities.
    /// </summary>
    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();
    
    /// <summary>
    /// Dictionary tracking remaining cooldown times for weapons.
    /// </summary>
    private Dictionary<WeaponData, float> weaponCooldowns = new Dictionary<WeaponData, float>();
    
    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Initializes hotbar slots and sets initial visibility.
    /// </summary>
    void Awake()
    {
        
        // Initialize slots
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].Initialize(i + 1);
            }
        }
        
        // Set initial visibility based on startVisible setting
        isHotbarVisible = startVisible;
        if (hotbarPanel != null)
            hotbarPanel.SetActive(startVisible);
    }
    
    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Finds and assigns the player reference, and configures the canvas for world space rendering.
    /// </summary>
    void Start()
    {
        // Find player in parent or by tag
        player = GetComponentInParent<Player>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        }
        
        if (player == null)
        {
            Debug.LogWarning("[HotbarManager] No Player found!");
        }
        
        // Setup canvas for world space if available
        if (hotbarCanvas != null)
        {
            hotbarCanvas.renderMode = RenderMode.WorldSpace;
            hotbarCanvas.worldCamera = Camera.main;
            
            var rectTransform = hotbarCanvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(8.2f, 1f);
            }
        }
    }
    
    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Handles input processing, cooldown updates, and hotbar positioning.
    /// </summary>
    void Update()
    {
        HandleInput();
        UpdateCooldowns();
        UpdatePosition();
    }
    
    /// <summary>
    /// Updates hotbar position to follow player.
    /// </summary>
    private void UpdatePosition()
    {
        if (player != null && hotbarCanvas != null && hotbarCanvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = player.transform.position + worldSpaceOffset;
            
            // Face camera
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }
    
    /// <summary>
    /// Handles keyboard input for hotbar toggle and item usage.
    /// </summary>
    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        var bindings = KeyBindings.Instance;
        if (bindings == null) return;
        
        // Toggle hotbar visibility
        if (keyboard[bindings.ToggleHotbar].wasPressedThisFrame)
        {
            ToggleHotbar();
        }
        
        // Allow using items even when hotbar UI is hidden (for quick access)
        // Skip if player is in minigame or shop
        if (player != null && player.IsInMinigameOrShop())
            return;
        
        // Check number keys 1-9
        if (keyboard.digit1Key.wasPressedThisFrame) UseSlot(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) UseSlot(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) UseSlot(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) UseSlot(3);
        else if (keyboard.digit5Key.wasPressedThisFrame) UseSlot(4);
        else if (keyboard.digit6Key.wasPressedThisFrame) UseSlot(5);
        else if (keyboard.digit7Key.wasPressedThisFrame) UseSlot(6);
        else if (keyboard.digit8Key.wasPressedThisFrame) UseSlot(7);
        else if (keyboard.digit9Key.wasPressedThisFrame) UseSlot(8);
    }
    
    /// <summary>
    /// Toggles hotbar visibility between shown and hidden states.
    /// </summary>
    public void ToggleHotbar()
    {
        isHotbarVisible = !isHotbarVisible;
        
        if (hotbarPanel != null)
            hotbarPanel.SetActive(isHotbarVisible);
        
        Debug.Log($"[HotbarManager] Hotbar {(isHotbarVisible ? "shown" : "hidden")}");
    }
    
    /// <summary>
    /// Shows the hotbar UI.
    /// </summary>
    public void ShowHotbar()
    {
        isHotbarVisible = true;
        if (hotbarPanel != null)
            hotbarPanel.SetActive(true);
    }
    
    /// <summary>
    /// Hides the hotbar UI.
    /// </summary>
    public void HideHotbar()
    {
        isHotbarVisible = false;
        if (hotbarPanel != null)
            hotbarPanel.SetActive(false);
    }
    
    /// <summary>
    /// Uses the item in the specified slot.
    /// Handles both weapon equipping and ability activation with cooldown checking.
    /// </summary>
    /// <param name="slotIndex">The zero-based index of the slot to use (0-8).</param>
    private void UseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSlots.Length) return;
        if (player == null) return;
        
        var slot = hotbarSlots[slotIndex];
        if (slot == null || slot.IsEmpty) return;
        
        // Handle weapon selection
        if (slot.WeaponData != null)
        {
            // Check if player owns this weapon
            if (player.OwnedWeapons.Any(w => w == slot.WeaponData))
            {
                // Check if on cooldown
                if (weaponCooldowns.ContainsKey(slot.WeaponData) && weaponCooldowns[slot.WeaponData] > 0f)
                {
                    Debug.Log($"[HotbarManager] Weapon {slot.WeaponData.displayName} is on cooldown: {weaponCooldowns[slot.WeaponData]:F1}s");
                    return;
                }
                
                // Equip the weapon
                var weaponSystem = player.GetComponentInChildren<WeaponSystem>();
                if (weaponSystem != null)
                {
                    weaponSystem.EquipWeapon(slot.WeaponData);
                    
                    // Only start cooldown if weapon has a cooldown greater than 0
                    if (slot.WeaponData.cooldown > 0f)
                    {
                        weaponCooldowns[slot.WeaponData] = slot.WeaponData.cooldown;
                    }
                    
                    Debug.Log($"[HotbarManager] Equipped weapon: {slot.WeaponData.displayName}");
                }
            }
            else
            {
                Debug.LogWarning($"[HotbarManager] Player does not own weapon: {slot.WeaponData.displayName}");
            }
        }
        // Handle ability activation
        else if (slot.AbilityData != null)
        {
            var ability = slot.AbilityData;
            
            // Check if on cooldown
            if (abilityCooldowns.ContainsKey(ability) && abilityCooldowns[ability] > 0f)
            {
                Debug.Log($"[HotbarManager] Ability {ability.displayName} is on cooldown: {abilityCooldowns[ability]:F1}s");
                return;
            }
            
            // Check if player knows this ability
            if (player.LearnedAbilities.Any(a => a == ability))
            {
                // Activate ability
                bool activated = player.ActivateAbility(ability);
                if (activated)
                {
                    // Start cooldown
                    abilityCooldowns[ability] = ability.cooldown;
                    Debug.Log($"[HotbarManager] Activated ability: {ability.displayName}");
                }
            }
            else
            {
                Debug.LogWarning($"[HotbarManager] Player has not learned ability: {ability.displayName}");
            }
        }
    }
    
    /// <summary>
    /// Updates ability and weapon cooldown timers and refreshes the UI display.
    /// Removes cooldowns that have completed and clears their UI indicators.
    /// </summary>
    private void UpdateCooldowns()
    {
        // Update ability cooldowns
        List<Ability> abilitiesToRemove = new List<Ability>();
        
        foreach (var kvp in abilityCooldowns.ToList()) // Convert to list to avoid modification during enumeration
        {
            float newTime = kvp.Value - Time.deltaTime;
            
            if (newTime <= 0f)
            {
                abilitiesToRemove.Add(kvp.Key);
            }
            else
            {
                abilityCooldowns[kvp.Key] = newTime;
            }
            
            // Update UI for this ability's slot
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null && hotbarSlots[i].AbilityData == kvp.Key)
                {
                    hotbarSlots[i].UpdateCooldown(newTime, kvp.Key.cooldown);
                }
            }
        }
        
        // Remove finished ability cooldowns
        foreach (var ability in abilitiesToRemove)
        {
            abilityCooldowns.Remove(ability);
            
            // Clear cooldown UI
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null && hotbarSlots[i].AbilityData == ability)
                {
                    hotbarSlots[i].UpdateCooldown(0f, ability.cooldown);
                }
            }
        }
        
        // Update weapon cooldowns
        List<WeaponData> weaponsToRemove = new List<WeaponData>();
        
        foreach (var kvp in weaponCooldowns.ToList()) // Convert to list to avoid modification during enumeration
        {
            float newTime = kvp.Value - Time.deltaTime;
            
            if (newTime <= 0f)
            {
                weaponsToRemove.Add(kvp.Key);
            }
            else
            {
                weaponCooldowns[kvp.Key] = newTime;
            }
            
            // Update UI for this weapon's slot
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null && hotbarSlots[i].WeaponData == kvp.Key)
                {
                    hotbarSlots[i].UpdateCooldown(newTime, kvp.Key.cooldown);
                }
            }
        }
        
        // Remove finished weapon cooldowns
        foreach (var weapon in weaponsToRemove)
        {
            weaponCooldowns.Remove(weapon);
            
            // Clear cooldown UI
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null && hotbarSlots[i].WeaponData == weapon)
                {
                    hotbarSlots[i].UpdateCooldown(0f, weapon.cooldown);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the hotbar manager for a specific player.
    /// </summary>
    /// <param name="player">The player whose hotbar manager to retrieve.</param>
    /// <returns>The HotbarManager component attached to the player, or null if not found.</returns>
    public static HotbarManager GetForPlayer(Player player)
    {
        if (player == null) return null;
        return player.GetComponentInChildren<HotbarManager>();
    }
    
    /// <summary>
    /// Adds a weapon to the first available slot.
    /// </summary>
    /// <param name="weapon">The weapon data to add to the hotbar.</param>
    /// <returns>True if the weapon was successfully added, false if it was already in the hotbar or no slots are available.</returns>
    public bool AddWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;
        
        // Check if weapon already in hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].WeaponData == weapon)
            {
                Debug.Log($"[HotbarManager] Weapon {weapon.displayName} already in hotbar at slot {i + 1}");
                return false;
            }
        }
        
        // Find first empty slot
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].IsEmpty)
            {
                hotbarSlots[i].SetWeapon(weapon);
                Debug.Log($"[HotbarManager] Added weapon {weapon.displayName} to slot {i + 1}");
                return true;
            }
        }
        
        Debug.LogWarning($"[HotbarManager] No empty slots available for weapon {weapon.displayName}");
        return false;
    }
    
    /// <summary>
    /// Adds an ability to the first available slot.
    /// </summary>
    /// <param name="ability">The ability to add to the hotbar.</param>
    /// <returns>True if the ability was successfully added, false if it was already in the hotbar or no slots are available.</returns>
    public bool AddAbility(Ability ability)
    {
        if (ability == null) return false;
        
        // Check if ability already in hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].AbilityData == ability)
            {
                Debug.Log($"[HotbarManager] Ability {ability.displayName} already in hotbar at slot {i + 1}");
                return false;
            }
        }
        
        // Find first empty slot
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].IsEmpty)
            {
                hotbarSlots[i].SetAbility(ability);
                Debug.Log($"[HotbarManager] Added ability {ability.displayName} to slot {i + 1}");
                return true;
            }
        }
        
        Debug.LogWarning($"[HotbarManager] No empty slots available for ability {ability.displayName}");
        return false;
    }
    
    /// <summary>
    /// Removes a weapon from the hotbar.
    /// </summary>
    /// <param name="weapon">The weapon to remove from the hotbar.</param>
    public void RemoveItem(WeaponData weapon)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].WeaponData == weapon)
            {
                hotbarSlots[i].Clear();
                Debug.Log($"[HotbarManager] Removed weapon from slot {i + 1}");
                return;
            }
        }
    }
    
    /// <summary>
    /// Removes an ability from the hotbar.
    /// </summary>
    /// <param name="ability">The ability to remove from the hotbar.</param>
    public void RemoveItem(Ability ability)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].AbilityData == ability)
            {
                hotbarSlots[i].Clear();
                Debug.Log($"[HotbarManager] Removed ability from slot {i + 1}");
                return;
            }
        }
    }
    
    /// <summary>
    /// Clears all hotbar slots and resets all cooldowns.
    /// </summary>
    public void ClearHotbar()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].Clear();
            }
        }
        abilityCooldowns.Clear();
        weaponCooldowns.Clear();
        Debug.Log("[HotbarManager] Cleared all hotbar slots");
    }
}
