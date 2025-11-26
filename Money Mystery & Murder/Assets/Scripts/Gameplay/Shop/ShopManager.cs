using UnityEngine;
using System;

public class ShopManager : MonoBehaviour
{
    public event Action<ShopItem> OnItemPurchased; // event-based notification

    public bool BuyItem(Player player, ShopItem item)
    {
        // TODO: Logic - validate player balance, deduct cost, grant item
        // On success:
        // OnItemPurchased?.Invoke(item);
        // if (hotbarUI != null) { /* hotbarUI.AddItemToHotbar(item); */ }
        throw new System.NotImplementedException();
    }

}
