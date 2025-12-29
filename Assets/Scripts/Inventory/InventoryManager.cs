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
        ItemSlotData handEquip = boxType == InventoryBox.InventoryType.Item ? equippedItemSlot : equippedToolSlot;
        ItemSlotData[] inventoryArr = boxType == InventoryBox.InventoryType.Item ? itemSlots : toolSlots;

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
        RefreshUIAndHand();

    }

    //movement of item from hand to inventory
    public void HandToInventory(InventoryBox.InventoryType boxType)
    {
        ItemSlotData handEquip = boxType == InventoryBox.InventoryType.Item ? equippedItemSlot : equippedToolSlot;
        ItemSlotData[] inventoryArr = boxType == InventoryBox.InventoryType.Item ? itemSlots : toolSlots;

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

        RefreshUIAndHand();


       
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
            RefreshUIAndHand();
            return true;
        }

        //Find an empty slot
        for(int i = 0; i < inventoryArr.Length; i++) {
            if(inventoryArr[i].IsEmpty()) {
                inventoryArr[i] = new ItemSlotData(itemSlotToMove);
                RefreshUIAndHand();
                return true;
            }
        }
        //No space in inventory
        return false;
    }

    // Centralized UI/hand refresh to keep visuals in sync
    private void RefreshUIAndHand()
    {
        RenderEquippedItem();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RenderInventory();
        }
    }


    //Render the item in the player's hand
    public void RenderEquippedItem()
    {
        if (handPoint == null) return;

        ClearHandPoint();

        // Prefer showing item, then tool
        if (SlotEquipped(InventoryBox.InventoryType.Item))
        {
            SpawnInHand(GetEquippedItemSlots(InventoryBox.InventoryType.Item));
            return;
        }

        if (SlotEquipped(InventoryBox.InventoryType.Tool))
        {
            SpawnInHand(GetEquippedItemSlots(InventoryBox.InventoryType.Tool));
        }
    }

    private void SpawnInHand(ItemData data)
    {
        if (data == null || data.onHandModel == null) return;
        Instantiate(data.onHandModel, handPoint);
    }

    private void ClearHandPoint()
    {
        // Destroy any existing children to avoid ghost hand models
        for (int i = handPoint.childCount - 1; i >= 0; i--)
        {
            Destroy(handPoint.GetChild(i).gameObject);
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
        if (item == null) return;

        bool isTool = IsTool(item);
        ItemSlotData target = isTool ? equippedToolSlot : equippedItemSlot;

        // Stack if the same item is already equipped
        if (target != null && !target.IsEmpty() && target.itemData == item)
        {
            target.AddQuantity(1);
        }
        else
        {
            if (isTool) equippedToolSlot = new ItemSlotData(item);
            else equippedItemSlot = new ItemSlotData(item);
        }

        RefreshUIAndHand();
    }

    public void EquipHandSlot(ItemSlotData slotData) {
        if (slotData == null || slotData.IsEmpty()) return;
        ItemData itemData = slotData.itemData;
        bool isTool = IsTool(itemData);
        ItemSlotData target = isTool ? equippedToolSlot : equippedItemSlot;

        if (target != null && target.Stackable(slotData)) {
            target.AddQuantity(slotData.quantity);
        } else {
            if (isTool) equippedToolSlot = new ItemSlotData(slotData);
            else equippedItemSlot = new ItemSlotData(slotData);
        }

        RefreshUIAndHand();
    }

    // Consume a quantity from a given slot (e.g., seeds when planting)
    public void ConsumeItem(ItemSlotData slot, int amount = 1)
    {
        if (slot == null || slot.IsEmpty()) return;
        slot.Remove(amount);
        RefreshUIAndHand();
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