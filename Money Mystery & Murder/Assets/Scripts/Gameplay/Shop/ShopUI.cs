using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Main UI controller for the shop panel. Displays a grid of purchasable items
/// (weapons and abilities) and manages the shop state.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemGridContainer;
    [SerializeField] private GameObject shopItemPrefab;
    
    [Header("Shop Configuration")]
    [SerializeField] private List<ShopItemData> shopItems = new();

    private Player _currentPlayer;
    private List<ShopItemUI> _itemUIInstances = new();
    private bool _isOpen;

    public bool IsOpen => _isOpen;
    public MonoBehaviour Host { get; private set; }

    /// <summary>
    /// Called by ShopActivator to initialize the shop.
    /// </summary>
    public void Initialize(MonoBehaviour host)
    {
        Host = host;
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        SetupShopItems();
    }

    /// <summary>
    /// Opens the shop UI for the given player.
    /// </summary>
    public void OpenShop(Player player)
    {
        if (_isOpen || player == null) return;

        _currentPlayer = player;
        _isOpen = true;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        DisablePlayerMovement();
        RefreshShopItems();
    }

    /// <summary>
    /// Closes the shop UI.
    /// </summary>
    public void CloseShop()
    {
        if (!_isOpen) return;

        _isOpen = false;

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        EnablePlayerMovement();
        _currentPlayer = null;
    }

    /// <summary>
    /// Called when a shop item is clicked/purchased.
    /// </summary>
    public void OnItemPurchased(ShopItemData itemData)
    {
        if (_currentPlayer == null || itemData == null) return;

        int price = itemData.GetPrice();
        
        if (!_currentPlayer.SpendBalance(price))
        {
            Debug.Log($"[ShopUI] Player {_currentPlayer.name} cannot afford {itemData.GetItemName()} (costs {price}, has {_currentPlayer.Balance})");
            return;
        }

        bool success = false;
        
        if (itemData.weapon != null)
        {
            _currentPlayer.AcquireWeapon(itemData.weapon);
            success = true;
            Debug.Log($"[ShopUI] Player {_currentPlayer.name} purchased weapon: {itemData.GetItemName()}");
        }
        else if (itemData.ability != null)
        {
            success = _currentPlayer.LearnAbility(itemData.ability);
            if (success)
            {
                Debug.Log($"[ShopUI] Player {_currentPlayer.name} purchased ability: {itemData.GetItemName()}");
            }
            else
            {
                _currentPlayer.AddBalance(price);
                Debug.Log($"[ShopUI] Failed to learn ability (already owned or invalid), refunding {price}");
            }
        }

        if (success)
        {
            RefreshShopItems();
        }
    }

    private void SetupShopItems()
    {
        if (itemGridContainer == null || shopItemPrefab == null) return;

        foreach (var itemUI in _itemUIInstances)
        {
            if (itemUI != null)
            {
                Destroy(itemUI.gameObject);
            }
        }
        _itemUIInstances.Clear();

        foreach (var itemData in shopItems)
        {
            if (itemData == null) continue;

            GameObject itemGO = Instantiate(shopItemPrefab, itemGridContainer);
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Initialize(itemData, this);
                _itemUIInstances.Add(itemUI);
            }
        }
    }

    private void RefreshShopItems()
    {
        foreach (var itemUI in _itemUIInstances)
        {
            if (itemUI != null)
            {
                itemUI.RefreshDisplay(_currentPlayer);
            }
        }
    }

    private void DisablePlayerMovement()
    {
        if (_currentPlayer == null) return;

        var movement = _currentPlayer.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        if (_currentPlayer == null) return;

        var movement = _currentPlayer.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }
    }
}

/// <summary>
/// Data structure for shop items that can contain either a weapon or an ability.
/// </summary>
[System.Serializable]
public class ShopItemData
{
    [Header("Item Content (Set one)")]
    public Weapon weapon;
    public AbilityDefinition ability;

    public string GetItemName()
    {
        if (weapon != null) return weapon.displayName;
        if (ability != null) return ability.displayName;
        return "Unknown Item";
    }

    public int GetPrice()
    {
        if (weapon != null) return weapon.cost;
        if (ability != null) return ability.cost;
        return 0;
    }

    public Sprite GetIcon()
    {
        if (weapon != null) return weapon.icon;
        if (ability != null) return ability.icon;
        return null;
    }
}
