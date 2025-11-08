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

    private void Start()
    {
        // Ensure raycast target is enabled
        if(backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
        }
        Debug.Log($"ShopListing Start called for {gameObject.name}, enabled: {enabled}");
    }
#region Active and Display
    public void Display(ItemData itemData)
    {
        this.itemData = itemData;
        this.enabled = true;
        
        gameObject.SetActive(true);

        if(backgroundImage != null) {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.enabled = true;
            backgroundImage.raycastTarget = true; // Ensure raycast target is enabled
        }
        
        if(nameText != null) {
            nameText.gameObject.SetActive(true);
            nameText.enabled = true;
            nameText.text = itemData.name;
        }

        if(costText != null) {
            costText.gameObject.SetActive(true);
            costText.enabled = true;
            costText.text = PlayerStats.CURRENCY + itemData.cost;
        }

        if(itemIcon != null) {
            itemIcon.gameObject.SetActive(true);
            itemIcon.enabled = true;
            itemIcon.sprite = itemData.icon;
        }
        Debug.Log($"Shop listing displayed: {itemData.name}, ShopListing enabled: {enabled}");
    }
#endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.shopListingManager.OpenConfirmPanel(itemData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.DisplayItemInfo(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        UIManager.Instance.DisplayItemInfo(null);
    }
}