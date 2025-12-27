using UnityEngine;
using System;
using System.Linq;

public class Shop : MonoBehaviour
{
    public event Action<ShopItem> OnItemPurchased;

    /// <summary>
    /// Attempts to purchase a shop item for the given player.
    /// Validates balance, deducts cost, and grants the item.
    /// </summary>
    public bool BuyItem(Player player, ShopItem item)
    {
        if (player == null || item == null)
        {
            Debug.LogWarning("[Shop] Cannot buy item: player or item is null");
            return false;
        }

        bool success = false;

        switch (item.ItemType)
        {
            case ShopItemType.Weapon:
                if (item is WeaponShopItem weaponItem && weaponItem.WeaponData != null)
                {
                    if (!player.SpendBalance(item.Price))
                    {
                        Debug.Log($"[Shop] Player {player.name} cannot afford {item.ItemName} (costs {item.Price}, has {player.Balance})");
                        return false;
                    }
                    
                    player.AcquireWeapon(weaponItem.WeaponData);
                    success = true;
                    Debug.Log($"[Shop] Player {player.name} purchased weapon: {item.ItemName}");
                }
                break;

            case ShopItemType.Upgrade:
                if (item is AbilityShopItem abilityItem && abilityItem.Ability != null)
                {
                    // Check if player can learn the ability (not already owned)
                    if (player.LearnedAbilities.Any(a => a == abilityItem.Ability))
                    {
                        Debug.Log($"[Shop] Player {player.name} already owns ability: {item.ItemName}");
                        return false;
                    }
                    
                    if (!player.SpendBalance(item.Price))
                    {
                        Debug.Log($"[Shop] Player {player.name} cannot afford {item.ItemName} (costs {item.Price}, has {player.Balance})");
                        return false;
                    }
                    
                    // Add ability directly without paying again
                    success = player.AcquireAbility(abilityItem.Ability);
                    if (success)
                    {
                        Debug.Log($"[Shop] Player {player.name} purchased ability: {item.ItemName}");
                    }
                    else
                    {
                        // Refund if something went wrong
                        player.AddBalance(item.Price);
                        Debug.Log($"[Shop] Failed to learn ability (invalid), refunding {item.Price}");
                    }
                }
                break;
        }

        if (success)
        {
            OnItemPurchased?.Invoke(item);
        }

        return success;
    }
}

/// <summary>
/// Shop item variant that contains a weapon.
/// </summary>
public class WeaponShopItem : ShopItem
{
    public WeaponData WeaponData;
}

/// <summary>
/// Shop item variant that contains an ability.
/// </summary>
public class AbilityShopItem : ShopItem
{
    public Ability Ability;
}
