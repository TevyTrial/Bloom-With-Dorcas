using UnityEngine;
using System.Collections;
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
            Debug.Log("OpenShopFromDialogue called");       
            
            // Small delay to ensure conversation closes properly
            Instance.StartCoroutine(Instance.OpenShopAfterDelay());
        }
        else
        {
            Debug.LogError("ShopScript instance is null!");
        }
    }

    private IEnumerator OpenShopAfterDelay()
    {
        // Wait for one frame to ensure conversation UI is closed
        yield return null;
        
        Debug.Log("Opening shop after delay");
        Pickup();
    }

    // Process purchase transaction
    public static void Purchase(ItemData item, int quantity) {
        int totalCost = item.cost * quantity;
        
        Debug.Log($"Attempting to purchase {quantity}x {item.name} for {totalCost}. Player has {PlayerStats.Money}");
        
        if(PlayerStats.Money >= totalCost) 
        {
            // Deduct money from player
            PlayerStats.Spend(totalCost);

            //Create item slot data
            ItemSlotData purchasedItem = new ItemSlotData(item, quantity);
            
            // Add item to player inventory
            bool success = InventoryManager.Instance.ShopToInventory(purchasedItem);
            
            if(success) {
                Debug.Log($"Purchased {quantity}x {item.name} for {totalCost}");
            } else {
                Debug.LogWarning("Failed to add item to inventory - inventory might be full");
                // Refund the money
                PlayerStats.Earn(totalCost);
            }
        }
        else
        {
            Debug.LogWarning($"Not enough money! Need {totalCost}, have {PlayerStats.Money}");
        }
    }

    public override void Pickup() {
        Debug.Log($"Pickup called. Shop has {(shopItems != null ? shopItems.Count : 0)} items");
        
        if (shopItems == null || shopItems.Count == 0)
        {
            Debug.LogError("Shop items list is empty or null!");
            return;
        }
        
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