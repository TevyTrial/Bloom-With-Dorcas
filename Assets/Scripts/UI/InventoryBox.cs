using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    ItemData itemToDisplay;

    public Image itemDisplayImage;

    public enum InventoryBoxType { Tool, Item }; 
    public InventoryBoxType boxType;

    int boxIndex;

    public void Display(ItemData itemToDisplay)
    {
        //Check if there is an item to display
        if(itemToDisplay != null)
        {
            //Switch the icon over
            itemDisplayImage.sprite = itemToDisplay.icon;
            this.itemToDisplay = itemToDisplay;

            itemDisplayImage.gameObject.SetActive(true);

            return; 
        }

        itemDisplayImage.gameObject.SetActive(false);     
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