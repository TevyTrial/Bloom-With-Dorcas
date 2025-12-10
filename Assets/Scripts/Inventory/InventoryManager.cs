using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    //The full list of items 
    public ItemIndex itemIndex;

    [Header("Tools")]
    //Tool Slots
    [SerializeField] private ItemSlotData[] toolSlots;
    //Tool in the player's hand
    [SerializeField] private ItemSlotData equippedToolSlot;

    [Header("Items")]
    //Item Slots
    [SerializeField] private ItemSlotData[] itemSlots;
    //Item in the player's hand
    [SerializeField] private ItemSlotData equippedItemSlot;

    //The transform for the player to hold the item
    public Transform handPoint;

    private void Awake()
{
    //If there is more than one instance, destroy the extra
    if(Instance != null && Instance != this)
    {
        Destroy(this);
    }
    else
    {
        //Set the static instance to this instance
        Instance = this; 
    }
    // Ensure equipped slots and inventory arrays are initialized to avoid null refs
    InitializeSlots();
}

// Ensure no null slots exist so methods can call IsEmpty()/Stackable() safely
void InitializeSlots()
{
    // Initialize arrays if null
    if (toolSlots == null) toolSlots = new ItemSlotData[10];
    if (itemSlots == null) itemSlots = new ItemSlotData[10];
    
    if (equippedToolSlot == null) equippedToolSlot = new ItemSlotData((ItemData)null, 0);
    if (equippedItemSlot == null) equippedItemSlot = new ItemSlotData((ItemData)null, 0);

    for (int i = 0; i < toolSlots.Length; i++)
    {
        if (toolSlots[i] == null) toolSlots[i] = new ItemSlotData((ItemData)null, 0);
    }
    for (int i = 0; i < itemSlots.Length; i++)
    {
        if (itemSlots[i] == null) itemSlots[i] = new ItemSlotData((ItemData)null, 0);
    }
}

    //movement of item from inventory to hand
    public void InventoryToHand(int boxIndex, InventoryBox.InventoryType boxType)
    {
        //The slot data in hand
        ItemSlotData handEquip = equippedToolSlot;
        //The array to change
        ItemSlotData[] inventoryArr = toolSlots;

        if(boxType == InventoryBox.InventoryType.Item) {
            //Store the item to a temp variable
            handEquip = equippedItemSlot;
            inventoryArr = itemSlots;
        }

        //Check if nothing is in the inventory slot
        if(inventoryArr[boxIndex].IsEmpty()) {
            return;
        }

        //Check if stackable
        if(handEquip.Stackable(inventoryArr[boxIndex])) {
            ItemSlotData slotToAlter = inventoryArr[boxIndex];
            
            //add the quantity to the hand slot
            handEquip.AddQuantity(slotToAlter.quantity);

            //empty the inventory slot
            slotToAlter.Empty();

        } else {
            //not stackable, swap the items
            //cache the inventory slot
            ItemSlotData slotToEquip = new ItemSlotData(inventoryArr[boxIndex]);

            //Change the inventory slot to the hand slot
            inventoryArr[boxIndex] = new ItemSlotData(handEquip);

            EquipHandSlot(slotToEquip);

        }
        //Update in scenes
        RenderEquippedItem();
        //Update the inventory UI
        UIManager.Instance.RenderInventory();

    }

    //movement of item from hand to inventory
    public void HandToInventory(InventoryBox.InventoryType boxType)
    {
        //The slot data in hand
        ItemSlotData handEquip = equippedToolSlot;
        //The array to change
        ItemSlotData[] inventoryArr = toolSlots;

        if(boxType == InventoryBox.InventoryType.Item) {
            handEquip = equippedItemSlot;
            inventoryArr = itemSlots;
        }

        //Check if stackable
        if(!StackableToInventory(handEquip, inventoryArr)) {
            //Find an empty slot if not stackable
            for(int i = 0; i < inventoryArr.Length; i++) {
                if(inventoryArr[i].IsEmpty()) {
                    inventoryArr[i] = new ItemSlotData(handEquip);
                    handEquip.Empty();
                    break;
                }
            }

        }

         //Update in scenes
        if(boxType == InventoryBox.InventoryType.Item) {
            RenderEquippedItem();
        }
        //Update the inventory UI
        UIManager.Instance.RenderInventory();


       
    }

    //Iterate through the inventory array and check if the item in hand is stackable with any item in the inventory
    public bool StackableToInventory(ItemSlotData itemSlot,ItemSlotData[] inventoryArr) {
        for(int i = 0; i < inventoryArr.Length; i++) {
            if(inventoryArr[i].Stackable(itemSlot)) {
                inventoryArr[i].AddQuantity(itemSlot.quantity);
                itemSlot.Empty();
                return true;
            }
        }
        return false;
    }
    
    // movement of item from shop to inventory
    public bool ShopToInventory(ItemSlotData itemSlotToMove) {
        //The inventory array to change
        ItemSlotData[] inventoryArr = IsTool(itemSlotToMove.itemData) ? toolSlots : itemSlots;

        //Check if stackable
        if(StackableToInventory(itemSlotToMove, inventoryArr)) {
            //Update the inventory UI
            UIManager.Instance.RenderInventory();
            RenderEquippedItem();
            return true;
        }

        //Find an empty slot
        for(int i = 0; i < inventoryArr.Length; i++) {
            if(inventoryArr[i].IsEmpty()) {
                inventoryArr[i] = new ItemSlotData(itemSlotToMove);
                //Update the inventory UI
                UIManager.Instance.RenderInventory();
                RenderEquippedItem();
                return true;
            }
        }
        //No space in inventory
        return false;
    }


    //Render the item in the player's hand
    public void RenderEquippedItem()
    {
        //Reset object in hand
        if(handPoint.childCount > 0)
        {
            Destroy(handPoint.GetChild(0).gameObject);
        }

        //Check if the player has an item equipped
        if(SlotEquipped(InventoryBox.InventoryType.Item))
        {
            //Instantiate the item model at the hand point
            ItemData data = GetEquippedItemSlots(InventoryBox.InventoryType.Item);
            if (data != null && data.onHandModel != null)
            {
                GameObject inst = Instantiate(data.onHandModel, handPoint);
            }
            return;
        }

        //Check if the player has an tool equipped
        if(SlotEquipped(InventoryBox.InventoryType.Tool))
        {
            //Instantiate the tool model at the hand point
            ItemData data = GetEquippedItemSlots(InventoryBox.InventoryType.Tool);
            if (data != null && data.onHandModel != null)
            {
                GameObject inst = Instantiate(data.onHandModel, handPoint);
            }
            return;
        }
    }

    //Inventory slot data accessors

    //Get function for the equipped item slots
    public ItemData GetEquippedItemSlots(InventoryBox.InventoryType InventoryType)
    {
        // Defensive: return null if the slot is null or empty
        if(InventoryType == InventoryBox.InventoryType.Item)
        {
            if (equippedItemSlot.IsEmpty()) return null;
            return equippedItemSlot.itemData;
        }
        if (equippedToolSlot.IsEmpty()) return null;
        return equippedToolSlot.itemData;

    }

    //Get function for the slots 
    public ItemSlotData GetEquippedSlot(InventoryBox.InventoryType InventoryType)
    {
       if(InventoryType == InventoryBox.InventoryType.Item)
        {
            return equippedItemSlot;
        }
        return equippedToolSlot;
    }

    //Get function for the inventory slots
    public ItemSlotData[] GetInventorySlots(InventoryBox.InventoryType InventoryType)
    {
        if(InventoryType == InventoryBox.InventoryType.Item)
        {
            return itemSlots;
        }
        return toolSlots;
    }

    //Check if slot in hand is equipped
    public bool SlotEquipped(InventoryBox.InventoryType inventoryType)
    {
        if(inventoryType == InventoryBox.InventoryType.Item)
        {
            return equippedItemSlot != null && !equippedItemSlot.IsEmpty();
        }
        return equippedToolSlot != null && !equippedToolSlot.IsEmpty();
    }

    //Check if the item is a tool
    public bool IsTool(ItemData item) {
        //Check if the item is of type EquipmentData
        //Try casting the itemdata as equipment data
        EquipmentData equipment = item as EquipmentData;
        if(equipment != null) {
            return true;
        }

        //Check if the item is a seed
        SeedData seed = item as SeedData;
        return seed != null;
    }

    //Equip the hand slot with the specified item
    public void EquipHandSlot(ItemData item)
    {
        bool isTool = IsTool(item);
        if(isTool) {
            equippedToolSlot = new ItemSlotData(item);
            return;
        } else {
            equippedItemSlot = new ItemSlotData(item);
        }
    }

    public void EquipHandSlot(ItemSlotData slotData)
    {
        //Get the item data from the slot
        ItemData itemData = slotData.itemData;
        bool isTool = IsTool(itemData);
        if(isTool) {
            equippedToolSlot = new ItemSlotData(slotData);
            return;
        } else {
            equippedItemSlot = new ItemSlotData(slotData);
        }
    }

    public void ConsumeItem(ItemSlotData itemSlot) {
        if(itemSlot.IsEmpty()) {
            Debug.Log("No item to consume");
            return;
        }

        itemSlot.Remove(1);
        RenderEquippedItem();
        UIManager.Instance.RenderInventory();
    }

    public void OnValidate() {
        //Validate the hand slots
        ValidateInventorySlots(equippedToolSlot);
        ValidateInventorySlots(equippedItemSlot);

        //Validate the inventory slots
        ValidateInventorySlots(toolSlots);
        ValidateInventorySlots(itemSlots);
    }

    //When giving the itemData value in the inspector, ensure the ItemSlotData arrays are valid
    void ValidateInventorySlots(ItemSlotData slot) {
        if(slot.itemData != null && slot.quantity == 0) {
            slot.quantity = 1;
        }
    }

    void ValidateInventorySlots(ItemSlotData[] array) {
        foreach(ItemSlotData slot in array) {
            ValidateInventorySlots(slot);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}