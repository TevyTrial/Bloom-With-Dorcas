using UnityEngine;
using System.Collections.Generic;

public class ShopScript : InteractableObject
{
    public List<ItemData> shopItems; // Items available in the shop

    // Process purchase transaction
    public static void Purchase(ItemData item, int quantity) {
        int totalCost = item.cost * quantity;
        
        if(PlayerStats.Money >= totalCost) 
        {
            // Deduct money from player
            PlayerStats.Spend(totalCost);

            //Create item slot data
            ItemSlotData purchasedItem = new ItemSlotData(item, quantity);
            
            // Add item to player inventory
            InventoryManager.Instance.ShopToInventory(purchasedItem);

        }
    }

    public override void Pickup() {
        Debug.Log("Purchasing");
        Purchase(item, 2);
    }


}