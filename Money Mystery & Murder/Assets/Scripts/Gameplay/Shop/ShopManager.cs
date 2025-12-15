using UnityEngine;
using System;

public class Shop : MonoBehaviour
{
    public event Action<ShopItem> OnItemPurchased; // event-based notification

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

        if (!player.SpendBalance(item.Price))
        {
            Debug.Log($"[Shop] Player {player.name} cannot afford {item.ItemName} (costs {item.Price}, has {player.Balance})");
            return false;
        }

        bool success = false;

        switch (item.ItemType)
        {
            case ShopItemType.Weapon:
                if (item is WeaponShopItem weaponItem && weaponItem.Weapon != null)
                {
                    player.AcquireWeapon(weaponItem.Weapon);
                    success = true;
                    Debug.Log($"[Shop] Player {player.name} purchased weapon: {item.ItemName}");
                }
                break;

            case ShopItemType.Upgrade:
                if (item is AbilityShopItem abilityItem && abilityItem.Ability != null)
                {
                    success = player.LearnAbility(abilityItem.Ability);
                    if (!success)
                    {
                        player.AddBalance(item.Price);
                        Debug.Log($"[Shop] Failed to learn ability (already owned), refunding {item.Price}");
                    }
                    else
                    {
                        Debug.Log($"[Shop] Player {player.name} purchased ability: {item.ItemName}");
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
    public Weapon Weapon;
}

/// <summary>
/// Shop item variant that contains an ability.
/// </summary>
public class AbilityShopItem : ShopItem
{
    public Ability Ability;
}
