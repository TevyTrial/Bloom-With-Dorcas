using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopListing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image itemIcon;

    ItemData itemData;

    public void Display(ItemData itemData)
    {
        this.itemData = itemData;
        
        // Make sure the listing itself is active FIRST
        gameObject.SetActive(true);
        
        // Activate and enable the background
        if(backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.enabled = true;
        }
        
        // Activate and enable the name text component
        if(nameText != null)
        {
            nameText.gameObject.SetActive(true);
            nameText.enabled = true;
            nameText.text = itemData.name;
        }
        
        // Activate and enable the cost text component
        if(costText != null)
        {
            costText.gameObject.SetActive(true);
            costText.enabled = true;
            costText.text = PlayerStats.CURRENCY + itemData.cost;
        }
        
        // Activate and enable the item icon component
        if(itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.enabled = true;
            itemIcon.sprite = itemData.icon;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip or highlight
        UIManager.Instance.DisplayItemInfo(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip or remove highlight
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}