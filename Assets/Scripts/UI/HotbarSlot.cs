using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private ShopItem assignedItem;

    public ShopItem AssignedItem => assignedItem;

    public void SetItem(ShopItem item)
    {
        // TODO: Logic - set item and update icon sprite/visibility
        throw new System.NotImplementedException();
    }

    public void ClearSlot()
    {
        // TODO: Logic - clear item and hide/reset icon
        throw new System.NotImplementedException();
    }
}
