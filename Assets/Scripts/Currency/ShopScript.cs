using UnityEngine;
using System.Collections.Generic;

public class ShopScript : InteractableObject
{
    public static ShopScript Instance { get; private set; }
    
    public List<ItemData> shopItems; // Items available in the shop

    private void Awake()
    {
        // Set up singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Static method that can be called from dialogue events
    public static void OpenShopFromDialogue()
    {
        if (Instance != null)
        {
            Instance.Pickup();
        }
        else
        {
            Debug.LogError("ShopScript instance is null!");
        }
    }

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
        Debug.Log("Purchasing - Opening shop with " + shopItems.Count + " items");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenShop(shopItems);
        }
        else
        {
            Debug.LogError("UIManager is null!");
        }
    }
}