using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShippingBin : InteractableObject
{
    public static List<ItemSlotData> itemsToShip = new List<ItemSlotData>();
    public Animator animator;

    public override void Pickup()
    {
        //Get the item data of the item the player is trying to throw in
        ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);

        //If the player is not holding anything, nothing should happen
        if (handSlotItem == null) return; 

        UIManager.Instance.HideSellingTooltip();
        
        //Open Yes No prompt to confirm if the player wants to sell
        UIManager.Instance.TriggerYesNoPrompt($"Do you want to sell {handSlotItem.name} ? ", PlaceItemsInShippingBin);

    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            // Check if player is holding an item
            ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            
            if (handSlotItem != null)
            {
                UIManager.Instance.ShowSellingTooltip();
            }
            if(animator != null) {
                animator.SetBool("IsOpen", true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.HideSellingTooltip();
        }
        if(animator != null) {
            animator.SetBool("IsOpen", false);
        }
    }

    void PlaceItemsInShippingBin()
    {
        //Get the ItemSlotData of what the player holding
        ItemSlotData handSlotItem = InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Item);       

        //Add the item to the shipping bin
        itemsToShip.Add(new ItemSlotData(handSlotItem));
        
        //Empty the player's hand slot
        handSlotItem.Empty();

        //Update the changes
        InventoryManager.Instance.RenderEquippedItem();

        foreach(ItemSlotData item in itemsToShip) {
            if(item != null && !item.IsEmpty()) {
                Debug.Log($"In the shipping bin: {item.itemData.name} x {item.quantity}");
            }
        }
        // Sell the items
        ShipItems();
    }

    public static void ShipItems() {
        //Tally up the total value of the items
        int totalEarnings = TallyItems(itemsToShip);
        //Ship the items
        PlayerStats.Earn(totalEarnings);
        //Empty the bin
        itemsToShip.Clear();
    }

    static int TallyItems(List<ItemSlotData> items) {
        int total = 0;

        foreach(ItemSlotData item in items) {  
            total += item.itemData.cost * item.quantity;
            Debug.Log($"Shipped {item.itemData.name} x {item.quantity} for {item.itemData.cost * item.quantity}");
        }

        return total;
    }
}