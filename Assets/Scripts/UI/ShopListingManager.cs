using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ShopListingManager : MonoBehaviour
{
    // The Shop Listing prefab to instantiate
    public GameObject shopListingPrefab;

    // The parent transform where shop listings will be added
    public Transform shopListingParent;

    // Variables for item purchase
    ItemData itemToBuy;
    int quantity;

    [Header("Shop Confirmation")] 
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public TextMeshProUGUI confirmQuantityText;
    public TextMeshProUGUI costCalculationText;
    public Button purchaseButton;

    public void RenderShop(List<ItemData> shopItems) {
        
        // Reset the listing if there are existing items
        if(shopListingParent.childCount > 0) {
            foreach(Transform child in shopListingParent) {
                Destroy(child.gameObject);
            }
        }

        // Create a new listing for each item in the shop
        foreach(ItemData shopitem in shopItems) {
            // Instantiate a new shop listing
            GameObject listingObj = Instantiate(shopListingPrefab, shopListingParent);
            
            // Assign it the shop item and display it
            ShopListing listing = listingObj.GetComponent<ShopListing>();

            listing.Display(shopitem);
        }
        
    }

    public void OpenConfirmPanel(ItemData item, int quantity) {
        this.itemToBuy = item;
        this.quantity = quantity;
        RenderConfirmationPanel();

    }

    public void RenderConfirmationPanel() {
        confirmPanel.SetActive(true);
        confirmText.text = $"Buy {itemToBuy.name}?";
        confirmQuantityText.text = "x" + quantity;
        int totalCost = itemToBuy.cost * quantity;
        int playerMoneyLeft = PlayerStats.Money - totalCost;

        // No enough money
        if(playerMoneyLeft < 0) {
            costCalculationText.text = "Not enough money!";
            purchaseButton.interactable = false;
            return;
        } else {
            purchaseButton.interactable = true;
            costCalculationText.text = $"{PlayerStats.Money} > {playerMoneyLeft}";
        }

    }   

    public void AddQuantity() {
        quantity++;
        RenderConfirmationPanel();
    }

    public void SubtractQuantity() {
        if(quantity > 1) {
            quantity--;
            RenderConfirmationPanel();
        }
    }

    public void ConfirmPurchase() {
        ShopScript.Purchase(itemToBuy, quantity);
        confirmPanel.SetActive(false);
    }

    public void CancelPurchase() {
        confirmPanel.SetActive(false);
    }
}