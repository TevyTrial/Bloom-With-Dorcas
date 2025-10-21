using UnityEngine;

[System.Serializable]
public class ItemSlotData
{
    public ItemData itemData;
    public int quantity;    

    //constructor
    public ItemSlotData(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
        ValidateQuantity();
    }

    //overloaded constructor for single item
    public ItemSlotData(ItemData itemData)
    {
        this.itemData = itemData;
        this.quantity = 1;
        ValidateQuantity();
    }

    public ItemSlotData(ItemSlotData slotToCopy) {
        itemData = slotToCopy.itemData;
        quantity = slotToCopy.quantity;
    }

    //Stacking method
    public void AddQuantity()
    {
        AddQuantity(1);
    }

    //Add specified amount to quantity
    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    //Remove specified amount from quantity
    public void Remove(int amount)
    {
        quantity--;
        ValidateQuantity();
    }

    //Validate quantity and empty if zero or less
    private void ValidateQuantity()
    {
        if (quantity <= 0 || itemData == null)
        {
            Empty();
        }
    }

    //Check if the item is stackable
    public bool Stackable(ItemSlotData slotToCompare)
    {
        return slotToCompare.itemData == itemData;
    
    }

    //Empty the slot
    public void Empty()
    {
        itemData = null;
        quantity = 0;
    }

    public bool IsEmpty()
    {
        return itemData == null;
    }
}
