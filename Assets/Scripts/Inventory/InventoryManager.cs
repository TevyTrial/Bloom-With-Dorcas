using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

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
    }

    [Header("Tools")]
    //Tool Slots
    public ItemData[] tools = new ItemData[10];
    //Tool in the player's hand
    public ItemData equippedTool = null; 

    [Header("Items")]
    //Item Slots
    public ItemData[] items = new ItemData[10];
    //Item in the player's hand
    public ItemData equippedItem = null;

    //The transform for the player to hold the item
    public Transform handPoint;

    //movement of item from inventory to hand
    public void InventoryToHand(int boxIndex, InventoryBox.InventoryBoxType boxType)
    {
        if(boxType == InventoryBox.InventoryBoxType.Tool)
        {
            //Store the tool to a temp variable
            ItemData toolToEquip = tools[boxIndex];

            //Swap the tool in hand with the tool in the inventory array
            tools [boxIndex] = equippedTool;

            //Change the tool in hand to the new tool
            equippedTool = toolToEquip;
        }
        else
        {
            ItemData itemToEquip = items[boxIndex];
            items[boxIndex] = equippedItem;
            equippedItem = itemToEquip;
            RenderEquippedItem();
        }

        //Update the inventory UI
        UIManager.Instance.RenderInventory();
    }

    //movement of item from hand to inventory
    public void HandToInventory(InventoryBox.InventoryBoxType boxType)
    {
        if(boxType == InventoryBox.InventoryBoxType.Tool)
        {
            if(equippedTool == null)
            {
                Debug.Log("No tool equipped to move to inventory.");
                return;
            }

            // Find the first empty tool slot
            for(int i = 0; i < tools.Length; i++)
            {
                if(tools[i] == null)
                {
                    tools[i] = equippedTool;
                    equippedTool = null;
                    RenderEquippedItem();
                    UIManager.Instance.RenderInventory();
                    return;
                }
            }

            Debug.Log("No empty tool slots available in inventory.");
        }
        else
        {
            if(equippedItem == null)
            {
                Debug.Log("No item equipped to move to inventory.");
                return;
            }

            // Find the first empty item slot
            for(int i = 0; i < items.Length; i++)
            {
                if(items[i] == null)
                {
                    items[i] = equippedItem;
                    equippedItem = null;
                    RenderEquippedItem();
                    UIManager.Instance.RenderInventory();
                    return;
                }
            }

            UIManager.Instance.RenderInventory();
        }
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
        if(equippedItem != null && equippedItem.onHandModel != null)
        {
            //Instantiate the item model at the hand point
            GameObject itemObj = Instantiate(equippedItem.onHandModel, handPoint);
            return;
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