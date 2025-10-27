using UnityEngine;
using UnityEngine.EventSystems;

public class HandInventorySlot : InventoryBox
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        // PROTECTION: Only move item from hand to inventory if we're NOT dragging
        // This prevents accidentally moving equipped items during drag operations
        if (eventData.dragging)
        {
            return;
        }

        // Display item info when hovering
        ItemSlotData equippedSlot = InventoryManager.Instance.GetEquippedSlot(boxType);
        if (equippedSlot != null && !equippedSlot.IsEmpty())
        {
            UIManager.Instance.DisplayItemInfo(equippedSlot.itemData);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // Click on equipped slot moves item back to inventory
        InventoryManager.Instance.HandToInventory(boxType);
    }
}
