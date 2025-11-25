using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShippingBin : InteractableObject
{
    public static List<ItemSlotData> itemsToShip = new List<ItemSlotData>();
    public Animator animator;
    private bool isPlayerInTrigger = false; //to track if player is in trigger
    private bool isPromptOpen = false; //to track if prompt UI is open

    public override void Pickup()
    {
        //Get the item data of the item the player is trying to throw in
        ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);

        //If the player is not holding anything, nothing should happen
        if (handSlotItem == null) return; 

        isPromptOpen = true;
        UIManager.Instance.HideSellingTooltip();
        
        //Open Yes No prompt to confirm if the player wants to sell
        UIManager.Instance.TriggerYesNoPrompt($"Do you want to sell {handSlotItem.name} ? ", OnPromptClosed);
    }

    void Update()
    {
        // Check if player is in trigger and presses Q
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.Q))
        {
            ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            
            if (handSlotItem != null)
            {
                isPromptOpen = true;
                UIManager.Instance.HideSellingTooltip();
                UIManager.Instance.TriggerYesNoPrompt($"Do you want to sell {handSlotItem.name} ? ", OnPromptClosed);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            
            // Check if player is holding an item
            ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            
            if (handSlotItem != null && !isPromptOpen)
            {
                UIManager.Instance.ShowSellingTooltip();
            }
            
            if(animator != null)
            {
                animator.SetBool("IsOpen", true);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !isPromptOpen)
        {
            // Update tooltip visibility based on equipped item
            ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            
            if (handSlotItem != null)
            {
                UIManager.Instance.ShowSellingTooltip();
            }
            else
            {
                UIManager.Instance.HideSellingTooltip();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            isPromptOpen = false;
            UIManager.Instance.HideSellingTooltip();
            
            if(animator != null)
            {
                animator.SetBool("IsOpen", false);
            }
        }
    }

    void OnPromptClosed()
    {
        PlaceItemsInShippingBin();
        isPromptOpen = false;
        
        // Check if player still has an item after selling, show tooltip if they do
        if (isPlayerInTrigger)
        {
            ItemData handSlotItem = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            if (handSlotItem != null)
            {
                UIManager.Instance.ShowSellingTooltip();
            }
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

        foreach(ItemSlotData item in itemsToShip)
        {
            if(item != null && !item.IsEmpty())
            {
                Debug.Log($"In the shipping bin: {item.itemData.name} x {item.quantity}");
            }
        }
        // Sell the items
        ShipItems();
    }

    public static void ShipItems()
    {
        //Tally up the total value of the items
        int totalEarnings = TallyItems(itemsToShip);
        //Ship the items
        PlayerStats.Earn(totalEarnings);
        //Empty the bin
        itemsToShip.Clear();
    }

    static int TallyItems(List<ItemSlotData> items)
    {
        int total = 0;

        foreach(ItemSlotData item in items)
        {  
            total += item.itemData.cost * item.quantity;
            Debug.Log($"Shipped {item.itemData.name} x {item.quantity} for {item.itemData.cost * item.quantity}");
        }

        return total;
    }
}