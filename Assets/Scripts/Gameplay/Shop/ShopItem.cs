using UnityEngine;

public enum ShopItemType
{
    Weapon,
    Upgrade
}

public class ShopItem : ScriptableObject
{
    public string ItemName;
    public int Price;
    public Sprite Icon;
    public ShopItemType ItemType;
}
