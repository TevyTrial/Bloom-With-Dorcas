using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    //The item information the GameObject is supposed to represent
    public ItemData item;

    public virtual void Pickup() {
        
        //Set the player's inventory to the item
        InventoryManager.Instance.EquipHandSlot(item);
        
        //Update the item in hand model
        InventoryManager.Instance.RenderEquippedItem();
        
        //Destroy the game object in the world
        Destroy(gameObject);
    }
}
