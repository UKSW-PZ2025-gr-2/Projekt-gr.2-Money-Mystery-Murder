using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private HotbarSlot[] slots;
    [SerializeField] private Player player; // local player reference

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        // TODO: Logic - listen for number keys and activate corresponding slot
        throw new System.NotImplementedException();
    }

    public void AddItemToHotbar(ShopItem item)
    {
        // TODO: Logic - find first empty slot and assign item
        throw new System.NotImplementedException();
    }

    public void ActivateSlot(int index)
    {
        // TODO: Logic - use item in slot index via player.UseItem
        throw new System.NotImplementedException();
    }

    public HotbarSlot[] GetSlots() => slots;
}
