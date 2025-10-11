using UnityEngine;
using UnityEngine.EventSystems;

public class HandInventorySlot : InventoryBox
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        //Move item from hand to inventory
        InventoryManager.Instance.HandToInventory(boxType);
    }
}
