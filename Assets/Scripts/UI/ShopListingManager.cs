using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ShopListingManager : MonoBehaviour
{
    // The Shop Listing prefab to instantiate
    [SerializeField] private GameObject shopListingPrefab;

    // The parent transform where shop listings will be added
    [SerializeField] private Transform shopListingParent;

    // Variables for item purchase
    ItemData itemToBuy;
    int quantity;

    [Header("Shop Confirmation")] 
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public TextMeshProUGUI confirmQuantityText;
    public TextMeshProUGUI costCalculationText;
    public Button purchaseButton;
    public Image confirmSeedIcon;

    [Header("Season Icons")]
    [SerializeField] private SeasonIconEntry[] seasonIcons;

    [System.Serializable]
    private struct SeasonIconEntry
    {
        public GameTimeStamp.Season season;
        public Sprite icon;
    }    

    public void RenderShop(List<ItemData> shopItems)
    {
        //Reset the listings if there was a previous one
        if(shopListingParent.childCount > 0)
        {
            foreach(Transform child in shopListingParent)
            {
                Destroy(child.gameObject); 
            }
        }

        //Create a new listing for every item
        foreach(ItemData shopItem in shopItems)
        {
            //Instantiate a shop listing prefab for the item
            GameObject listingGameObject = Instantiate(shopListingPrefab, shopListingParent);

            //Get the ShopListing component
            ShopListing listing = listingGameObject.GetComponent<ShopListing>();
            
            // Display item info
            listing.Display(shopItem);
            
            // Set season icon if applicable
            Sprite seasonSprite = GetSeasonSprite(shopItem);
            listing.SetSeasonIcon(seasonSprite);
        }
    }

    public Sprite GetSeasonSprite(ItemData itemData) 
    {
        if(itemData is SeedData seedData) 
        {
            foreach(var entry in seasonIcons) 
            {
                if(entry.season == seedData.cropSeason) 
                {
                    return entry.icon;
                }
            }
        }
        return null; // No season icon
    }

    

    public void OpenConfirmPanel(ItemData item) {
        Debug.Log($"Opening confirm panel for {item.name}");
        this.itemToBuy = item;
        this.quantity = 1;
        RenderConfirmationPanel();
    }

    public void RenderConfirmationPanel() {
        confirmPanel.SetActive(true);
        confirmText.text = $"Buy {itemToBuy.name}?";
        confirmQuantityText.text = "x" + quantity;
        int totalCost = itemToBuy.cost * quantity;
        int playerMoneyLeft = PlayerStats.Money - totalCost;
        confirmSeedIcon.sprite = itemToBuy.icon;

        Debug.Log($"Total cost: {totalCost}, Player money: {PlayerStats.Money}, Money left: {playerMoneyLeft}");

        // Not enough money
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
        }
        RenderConfirmationPanel();
    }

    public void ConfirmPurchase() {
        Debug.Log($"Confirming purchase of {quantity}x {itemToBuy.name}");
        ShopScript.Purchase(itemToBuy, quantity);
        confirmPanel.SetActive(false);
    }

    public void CancelPurchase() {
        Debug.Log("Purchase cancelled");
        confirmPanel.SetActive(false);
    }
}