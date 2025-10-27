using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    ItemData itemToDisplay;
    int quantity;

    public Image itemDisplayImage;
    public TextMeshProUGUI quantityText;

    public enum InventoryType { Tool, Item }; 
    public InventoryType boxType;

    protected int boxIndex;
    public int BoxIndex => boxIndex;

    public void Display(ItemSlotData itemSlot)
    {
        //Set the variable name
        itemToDisplay = itemSlot.itemData;
        quantity = itemSlot.quantity;

        //Default quantity
        quantityText.text = "";

        //Check if there is an item to display
        if(itemToDisplay != null)
        {
            //Switch the icon over
            itemDisplayImage.sprite = itemToDisplay.icon;
            
            //Set the quantity text if more than 1
            if(quantity > 1)
            {
                quantityText.text = quantity.ToString();
            }

            itemDisplayImage.gameObject.SetActive(true);

            return; 
        }

        itemDisplayImage.gameObject.SetActive(false);     
    }

    public ItemSlotData GetSlotData()
    {
        ItemSlotData[] slots = InventoryManager.Instance.GetInventorySlots(boxType);
        if(slots != null && boxIndex >= 0 && boxIndex < slots.Length)
        {
            return slots[boxIndex];
        }
        return null;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.InventoryToHand(boxIndex, boxType);
    }
    
    //Assign the index of this box in the inventory array
    public void AssignIndex(int boxIndex)
    {
        this.boxIndex = boxIndex; 
    }

    //Display the item info on the item info box when the player mouses over
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.DisplayItemInfo(itemToDisplay);
    }

    //Reset the item info box when the player leaves
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.DisplayItemInfo(null);
    }
}