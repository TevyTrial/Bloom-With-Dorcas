using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class UIManager : MonoBehaviour, ITimeTracker
{
    public static UIManager Instance { get; private set; }
    [Header("Setus Bar")]
    //Tool equip box
    public Image toolEquipSlot;
    public TextMeshProUGUI toolEquipQuantityText;

    //Time display text
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;

    [Header("Currency Display")]
    public TextMeshProUGUI currencyText;

    [Header("Inventory System")]
    //The inventory panel
    public GameObject inventoryPanel; 

    //The tool slot UIs
    public InventoryBox[] toolSlots;

    //The tool equip slot UI
    public HandInventorySlot toolEquip;

    //The item slot UIs
    public InventoryBox[] itemSlots;

    //The item equip slot UI
    public HandInventorySlot itemEquip;

    //Item info box
    public TextMeshProUGUI itemTitle;

    public TextMeshProUGUI itemDescription;

    [Header("Tooltip System")]
    //Tooltip panel 
    public GameObject HarvestTooltipPanel;
    public GameObject InteractTooltipPanel;
/*
    [Header("Screen Transitions")]
    public GameObject fadeIn;
    public GameObject fadeOut;
*/
    [Header("Shop System")]
    public GameObject SellPanel;
    public GameObject BuyPanel;

    private void Awake()
    {
        //If there is more than one instance, destroy the extra
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            //Set the static instance to this instance
            Instance = this;
        }
    }

    private void Start()
    {
        RenderInventory();
        AssignBoxIndexes();
        RenderPlayerStats();

        //Register as a listener to time updates
        TimeManager.Instance.RegisterListener(this);
    }
/*
    public void FadeInScreen() {
        fadeIn.SetActive(true);
    }

    public void FadeOutScreen() {
        fadeOut.SetActive(true);
    }

    public void OnFadeInComplete() {
        //Disable Fade in screen
        fadeIn.SetActive(false);
    }

    public void ResetFadeDefault() {
        fadeOut.SetActive(false);
        fadeIn.SetActive(true);
    }
    */

    //iterate and assign indexes to each inventory box
    void AssignBoxIndexes()
    {
        //Assign indexes to tool slots
        for(int i = 0; i < toolSlots.Length; i++)
        {
            toolSlots[i].AssignIndex(i);
            itemSlots[i].AssignIndex(i);
        }

    }   

    //Render the inventory screen to reflect the Player's Inventory. 
    public void RenderInventory()
    {
        ItemSlotData[] inventoryToolSlots = InventoryManager.Instance.GetInventorySlots(InventoryBox.InventoryType.Tool);
        ItemSlotData[] inventoryItemSlots = InventoryManager.Instance.GetInventorySlots(InventoryBox.InventoryType.Item);
        //Render the Tool section
        RenderInventoryPanel(inventoryToolSlots, toolSlots);

        //Render the Item section
        RenderInventoryPanel(inventoryItemSlots, itemSlots);

        //Render the tool in hand section
        toolEquip.Display(InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Tool));
        //Render the item in hand section
        itemEquip.Display(InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Item));

        //Get Tools from inventory Manager
        ItemData equippedTool = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Tool);

        //Get the quantity of the equipped tool
        toolEquipQuantityText.text = "";

        //check if there is item to display
        if(equippedTool != null) {
            //Switch the icon over
            toolEquipSlot.sprite = equippedTool.icon;

            toolEquipSlot.gameObject.SetActive(true);

            //Get quantity
            int quantity = InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Tool).quantity;

            if(quantity > 1) {
                toolEquipQuantityText.text = quantity.ToString();
            }

            return; 
        }

        toolEquipSlot.gameObject.SetActive(false);
        
    }

    //Iterate through a slot in a section and display them in the UI
    void RenderInventoryPanel(ItemSlotData[] slots, InventoryBox[] uiSlots)
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            //Display them accordingly
            uiSlots[i].Display(slots[i]);
        }
    }

    public void ToggleInventoryPanel()
    {
        //If the panel is hidden, show it and vice versa
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        RenderInventory();
    }

    void Update()
    {
        // Check for inventory toggle shortcut (E key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventoryPanel();
        }
    
        // Check for escape key to close inventory panel
        if (Input.GetKeyDown(KeyCode.Escape) && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            RenderInventory();
        }
    }

    //Display Item info on the Item infobox
    public void DisplayItemInfo(ItemData data)
    {
        //If data is null, reset
        if(data == null)
        {
            itemTitle.text = "";
            itemDescription.text = "";

            return;
        }

        itemTitle.text = data.name;
        itemDescription.text = data.description; 
    }

    //Show harvest tooltip
    public void ShowHarvestTooltip()
    {
        if(HarvestTooltipPanel != null)
        {
            HarvestTooltipPanel.SetActive(true);
        }
    }
    //Hide harvest tooltip
    public void HideHarvestTooltip()
    {
        if(HarvestTooltipPanel != null)
        {
            HarvestTooltipPanel.SetActive(false);
        }
    }

    //Show interact tooltip
    public void ShowInteractTooltip()
    {
        Debug.Log("Show Interact Tooltip");
        if(InteractTooltipPanel != null)
        {
            InteractTooltipPanel.SetActive(true);
        }
    }
    //Hide interact tooltip
    public void HideInteractTooltip()
    {
        Debug.Log("Hide Interact Tooltip");
        if(InteractTooltipPanel != null)
        {
            InteractTooltipPanel.SetActive(false);
        }
    }

    // Show sell panel
    public void ShowSellPanel()
    {
        if(SellPanel != null)
        {
            SellPanel.SetActive(true);
        }
    }
    // Hide sell panel
    public void HideSellPanel()
    {
        if(SellPanel != null)
        {
            SellPanel.SetActive(false);
        }
    }
    // Show buy panel
    public void ShowBuyPanel()
    {
        if(BuyPanel != null)
        {
            BuyPanel.SetActive(true);
        }
    }
    // Hide buy panel
    public void HideBuyPanel()  
    {
        if(BuyPanel != null)
        {
            BuyPanel.SetActive(false);
        }
    }


    //Update the time display in the UI
    public void ClockUpdate(GameTimeStamp currentTime)
    {
        // Update the UI elements to reflect the current in-game time
        // Example: Update a UI text element with currentTime.hour and currentTime.minute
        
        //Get the hour and minute 
        int hour = currentTime.hour;
        int minute = currentTime.minute;

        //Format the time string (e.g., "HH:MM AM/PM")
        string period = "AM ";
        if(hour > 12){
            period = "PM ";
            hour -= 12; //Convert to 12-hour format
        }

        //Update the time text
        timeText.text = period + hour + ":" + minute.ToString("00");
        
        //Update the date text
        int day = currentTime.day;
        string season = currentTime.season.ToString().Substring(0,3); //Get first 3 letters of the season
        string dayOfWeek = currentTime.GetDayOfWeek().ToString().Substring(0,3); //Get first 3 letters of the day of the week
        dateText.text = season + " " + day + " (" + dayOfWeek + ")" ;
    }

    //Render player stats such as currency
    public void RenderPlayerStats()
    {
        //Update the currency text
        currencyText.text = PlayerStats.CURRENCY;
    }
    
}