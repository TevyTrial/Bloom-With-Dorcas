using UnityEngine;
using System.Collections.Generic;

public class ShopScript : InteractableObject
{
    public List<ItemData> shopItems; // Items available in the shop

    // Called from dialogue option node
    public void OpenBuyMode() 
    {
        Debug.Log("Opening buy mode");
        UIManager.Instance.ShowBuyPanel();
        UIManager.Instance.HideSellPanel();
        
        // TODO: Open shop UI with items for sale
        // ShopUIManager.Instance.OpenBuyMode(itemsForSale, buyPriceMultiplier);
        
    }

    // Called from dialogue option node
    public void OpenSellMode() 
    {
        Debug.Log("Opening sell mode");
        UIManager.Instance.ShowSellPanel();
        UIManager.Instance.HideBuyPanel();
        
        // TODO: Open shop UI showing player's inventory
        // ShopUIManager.Instance.OpenSellMode(sellPriceMultiplier);

    }

    public void CloseShop() 
    {
        Debug.Log("Closing shop");
        UIManager.Instance.HideBuyPanel();
        UIManager.Instance.HideSellPanel();

    }
    

    // Process purchase transaction
    public static void BuyItem(ItemData item, int quantity) 
    {
        int totalCost = item.price * quantity;
        
        if(PlayerStats.Money >= totalCost) 
        {
            // Deduct money from player
            PlayerStats.Spend(totalCost);

            //Create item slot data
            ItemSlotData purchasedSlot = new ItemSlotData(item, quantity);
            
            // Add item to player inventory
        

        }
    }

}