using UnityEngine;

/// <summary>
/// Enum defining the types of items available in the shop.
/// </summary>
public enum ShopItemType
{
    /// <summary>
    /// A weapon item that can be equipped.
    /// </summary>
    Weapon,
    
    /// <summary>
    /// An upgrade item that enhances player capabilities.
    /// </summary>
    Upgrade
}

/// <summary>
/// ScriptableObject representing an item that can be purchased in the shop.
/// Contains basic item information like name, price, icon, and type.
/// </summary>
public class ShopItem : ScriptableObject
{
    /// <summary>
    /// The display name of the item.
    /// </summary>
    public string ItemName;
    
    /// <summary>
    /// The price of the item in the game currency.
    /// </summary>
    public int Price;
    
    /// <summary>
    /// The icon sprite displayed for this item in the shop UI.
    /// </summary>
    public Sprite Icon;
    
    /// <summary>
    /// The type of item (Weapon or Upgrade).
    /// </summary>
    public ShopItemType ItemType;
}
