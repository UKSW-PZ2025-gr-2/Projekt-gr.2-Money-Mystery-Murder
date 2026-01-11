using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single slot in the hotbar that can hold either a weapon or an ability.
/// Displays the item's icon and keybind number.
/// </summary>
public class HotbarSlot : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Image component that displays the icon of the weapon or ability in this slot.
    /// </summary>
    [SerializeField] private Image itemIcon;
    
    /// <summary>
    /// Text component that displays the keybind number (1-9) for this slot.
    /// </summary>
    [SerializeField] private TMP_Text keyNumberText;
    
    /// <summary>
    /// Overlay GameObject shown when the slot is empty.
    /// </summary>
    [SerializeField] private GameObject emptySlotOverlay;
    
    /// <summary>
    /// Image overlay that displays the cooldown progress as a fill amount.
    /// </summary>
    [SerializeField] private Image cooldownOverlay;
    
    /// <summary>
    /// Text component that displays the remaining cooldown time in seconds.
    /// </summary>
    [SerializeField] private TMP_Text cooldownText;
    
    /// <summary>
    /// The weapon data stored in this slot, if any.
    /// </summary>
    private WeaponData weaponData;
    
    /// <summary>
    /// The ability data stored in this slot, if any.
    /// </summary>
    private Ability abilityData;
    
    /// <summary>
    /// The slot number (1-9) used for keybind display.
    /// </summary>
    private int slotNumber;
    
    /// <summary>
    /// Gets the weapon data in this slot.
    /// </summary>
    public WeaponData WeaponData => weaponData;
    
    /// <summary>
    /// Gets the ability data in this slot.
    /// </summary>
    public Ability AbilityData => abilityData;
    
    /// <summary>
    /// Gets whether this slot is empty (contains neither weapon nor ability).
    /// </summary>
    public bool IsEmpty => weaponData == null && abilityData == null;
    
    /// <summary>
    /// Gets the slot number (1-9).
    /// </summary>
    public int SlotNumber => slotNumber;
    
    /// <summary>
    /// Initializes the slot with its number (1-9).
    /// </summary>
    /// <param name="number">The slot number to assign.</param>
    public void Initialize(int number)
    {
        slotNumber = number;
        if (keyNumberText != null)
            keyNumberText.text = number.ToString();
        
        Clear();
    }
    
    /// <summary>
    /// Assigns a weapon to this slot and updates the icon.
    /// </summary>
    /// <param name="weapon">The weapon data to assign to this slot.</param>
    public void SetWeapon(WeaponData weapon)
    {
        Clear();
        weaponData = weapon;
        
        if (weapon != null && itemIcon != null)
        {
            itemIcon.sprite = weapon.icon;
            itemIcon.enabled = true;
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Assigns an ability to this slot and updates the icon.
    /// </summary>
    /// <param name="ability">The ability to assign to this slot.</param>
    public void SetAbility(Ability ability)
    {
        Clear();
        abilityData = ability;
        
        if (ability != null && itemIcon != null)
        {
            itemIcon.sprite = ability.icon;
            itemIcon.enabled = true;
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Clears the slot, removing any weapon or ability and resetting visuals.
    /// </summary>
    public void Clear()
    {
        weaponData = null;
        abilityData = null;
        
        if (itemIcon != null)
            itemIcon.enabled = false;
        
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;
        
        if (cooldownText != null)
            cooldownText.text = "";
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Updates the cooldown display with remaining time and progress.
    /// </summary>
    /// <param name="remainingTime">The remaining cooldown time in seconds.</param>
    /// <param name="totalCooldown">The total cooldown duration in seconds.</param>
    public void UpdateCooldown(float remainingTime, float totalCooldown)
    {
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = remainingTime / totalCooldown;
        }
        
        if (cooldownText != null)
        {
            if (remainingTime > 0f)
            {
                cooldownText.text = Mathf.Ceil(remainingTime).ToString();
            }
            else
            {
                cooldownText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Updates visual state based on whether slot is empty.
    /// </summary>
    private void UpdateVisuals()
    {
        if (emptySlotOverlay != null)
            emptySlotOverlay.SetActive(IsEmpty);
    }
    
    /// <summary>
    /// Gets the display name of the item in this slot.
    /// </summary>
    /// <returns>The display name of the weapon or ability, or "Empty" if the slot is empty.</returns>
    public string GetItemName()
    {
        if (weaponData != null)
            return weaponData.displayName;
        if (abilityData != null)
            return abilityData.displayName;
        return "Empty";
    }
}
