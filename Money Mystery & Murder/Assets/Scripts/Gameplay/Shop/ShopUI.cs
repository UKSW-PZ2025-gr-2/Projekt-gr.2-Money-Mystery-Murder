using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [SerializeField] private bool autoLoadAllWeapons = true;
    [SerializeField] private bool autoLoadAllAbilities = false;

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
        
        if (shopPanel == null)
        {
            Debug.LogError($"[ShopUI] shopPanel is not assigned on '{gameObject.name}'! Shop UI will not be visible.");
        }
        else
        {
            shopPanel.SetActive(false);
        }
        
        // Auto-load weapons and abilities if enabled
        if (autoLoadAllWeapons || autoLoadAllAbilities)
        {
            LoadItemsFromResources();
        }
        
        SetupShopItems();
    }

    /// <summary>
    /// Opens the shop UI for the given player.
    /// </summary>
    public void OpenShop(Player player)
    {
        if (_isOpen)
        {
            Debug.LogWarning($"[ShopUI] Shop is already open on '{gameObject.name}'");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError($"[ShopUI] Cannot open shop on '{gameObject.name}' - player is null");
            return;
        }

        _currentPlayer = player;
        _isOpen = true;

        if (shopPanel != null)
        {
            Debug.Log($"[ShopUI] Activating shop panel on '{gameObject.name}' for player '{player.name}'");
            shopPanel.SetActive(true);
        }
        else
        {
            Debug.LogError($"[ShopUI] shopPanel is null on '{gameObject.name}' - cannot display shop UI!");
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
            // Use AcquireAbility since we already paid
            success = _currentPlayer.AcquireAbility(itemData.ability);
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

    private void LoadItemsFromResources()
    {
        if (autoLoadAllWeapons)
        {
            WeaponData[] weapons = Resources.LoadAll<WeaponData>("");
            foreach (var weapon in weapons)
            {
                if (weapon != null && weapon.cost > 0)
                {
                    shopItems.Add(new ShopItemData { weapon = weapon });
                }
            }
            Debug.Log($"[ShopUI] Auto-loaded {weapons.Length} weapons from Resources");
        }

        if (autoLoadAllAbilities)
        {
            Ability[] abilities = Resources.LoadAll<Ability>("");
            foreach (var ability in abilities)
            {
                if (ability != null && ability.cost > 0)
                {
                    shopItems.Add(new ShopItemData { ability = ability });
                }
            }
            Debug.Log($"[ShopUI] Auto-loaded {abilities.Length} abilities from Resources");
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

#if UNITY_EDITOR
    [ContextMenu("Auto-Load All Weapons From Assets")]
    private void EditorLoadAllWeapons()
    {
        shopItems.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:WeaponData");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            
            if (weapon != null && weapon.cost > 0)
            {
                shopItems.Add(new ShopItemData { weapon = weapon });
                count++;
            }
        }
        
        Debug.Log($"[ShopUI] Loaded {count} weapons with cost > 0 into shop");
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Auto-Load All Abilities From Assets")]
    private void EditorLoadAllAbilities()
    {
        string[] guids = AssetDatabase.FindAssets("t:Ability");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Ability ability = AssetDatabase.LoadAssetAtPath<Ability>(path);
            
            if (ability != null && ability.cost > 0)
            {
                shopItems.Add(new ShopItemData { ability = ability });
                count++;
            }
        }
        
        Debug.Log($"[ShopUI] Loaded {count} abilities with cost > 0 into shop");
        EditorUtility.SetDirty(this);
    }
#endif
}

/// <summary>
/// Data structure for shop items that can contain either a weapon or an ability.
/// </summary>
[System.Serializable]
public class ShopItemData
{
    [Header("Item Content (Set one)")]
    public WeaponData weapon;
    public Ability ability;

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

    public string GetDescription()
    {
        if (weapon != null) return weapon.description;
        if (ability != null) return ability.description;
        return "";
    }
}
