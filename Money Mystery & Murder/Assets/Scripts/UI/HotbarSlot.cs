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
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text keyNumberText;
    [SerializeField] private GameObject emptySlotOverlay;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;
    
    private WeaponData weaponData;
    private Ability abilityData;
    private int slotNumber;
    
    public WeaponData WeaponData => weaponData;
    public Ability AbilityData => abilityData;
    public bool IsEmpty => weaponData == null && abilityData == null;
    public int SlotNumber => slotNumber;
    
    /// <summary>
    /// Initializes the slot with its number (1-9).
    /// </summary>
    public void Initialize(int number)
    {
        slotNumber = number;
        if (keyNumberText != null)
            keyNumberText.text = number.ToString();
        
        Clear();
    }
    
    /// <summary>
    /// Assigns a weapon to this slot.
    /// </summary>
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
    /// Assigns an ability to this slot.
    /// </summary>
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
    /// Clears the slot.
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
    /// Updates the cooldown display for abilities.
    /// </summary>
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
    public string GetItemName()
    {
        if (weaponData != null)
            return weaponData.displayName;
        if (abilityData != null)
            return abilityData.displayName;
        return "Empty";
    }
}
