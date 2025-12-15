using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// UI component for a single shop item displayed in the shop grid.
/// Shows the item's icon, name, and price, and handles purchase interaction.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button purchaseButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color ownedColor = new Color(0.3f, 0.7f, 0.3f, 1f);

    private ShopItemData _itemData;
    private ShopUI _shopUI;

    /// <summary>
    /// Initializes the shop item UI with data and parent shop reference.
    /// </summary>
    public void Initialize(ShopItemData itemData, ShopUI shopUI)
    {
        _itemData = itemData;
        _shopUI = shopUI;

        if (iconImage != null && itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
        }

        if (nameText != null)
        {
            nameText.text = itemData.itemName;
        }

        if (priceText != null)
        {
            priceText.text = $"${itemData.price}";
        }

        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
    }

    /// <summary>
    /// Refreshes the display state based on player's balance and owned items.
    /// </summary>
    public void RefreshDisplay(Player player)
    {
        if (player == null || _itemData == null) return;

        bool canAfford = player.Balance >= _itemData.price;
        bool alreadyOwned = CheckIfOwned(player);

        if (purchaseButton != null)
        {
            purchaseButton.interactable = canAfford && !alreadyOwned;
        }

        Color targetColor = affordableColor;
        if (alreadyOwned)
        {
            targetColor = ownedColor;
            if (priceText != null)
            {
                priceText.text = "OWNED";
            }
        }
        else if (!canAfford)
        {
            targetColor = unaffordableColor;
            if (priceText != null)
            {
                priceText.text = $"${_itemData.price}";
            }
        }
        else
        {
            if (priceText != null)
            {
                priceText.text = $"${_itemData.price}";
            }
        }

        if (iconImage != null)
        {
            iconImage.color = targetColor;
        }

        if (nameText != null)
        {
            nameText.color = targetColor;
        }
    }

    private void OnPurchaseClicked()
    {
        if (_shopUI != null && _itemData != null)
        {
            _shopUI.OnItemPurchased(_itemData);
        }
    }

    private bool CheckIfOwned(Player player)
    {
        if (player == null || _itemData == null) return false;

        if (_itemData.weapon != null)
        {
            return player.OwnedWeapons.Contains(_itemData.weapon);
        }

        if (_itemData.ability != null)
        {
            return player.LearnedAbilities.Contains(_itemData.ability);
        }

        return false;
    }

    private void OnDestroy()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
        }
    }
}
