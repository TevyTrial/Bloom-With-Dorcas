using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    PlayerController playerController;

    Land selectedLand = null;

    //The interactable object the player is currently looking at
    InteractableObject selectedInteractable = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        // Raycast forward from player position to detect objects in front
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f)) {
            OnInteractWithObject(hit);                    
        }
        // Also keep downward raycast for land detection
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, 1)) {
            OnInteractWithObject(hit);                    
        }

    }

    //check if the selection of the land
    void OnInteractWithObject(RaycastHit hit) {
        Collider collider = hit.collider;
        if(collider.tag == "Land") {
            Land land = collider.GetComponent<Land>();
            SelectLand(land);
            return;
        }

        //Check if looking at an interactable object
        if(collider.tag == "Item") {
            //Set the interactable to the currently looked at object
            selectedInteractable = collider.GetComponent<InteractableObject>();
            
            //Show tooltip for the item
            if(selectedInteractable != null && selectedInteractable.item != null) {
                UIManager.Instance.ShowTooltip();
            }

            return;
        }

        //Deselect the old interactable
        if(selectedInteractable != null) {
            selectedInteractable = null;
            UIManager.Instance.HideTooltip();
        }

        //deselect the old land
        if(selectedLand != null) {
            selectedLand.Select(false);
            selectedLand = null;
        }

    }

    void SelectLand(Land land) {
        //deselect the old land
        if(selectedLand != null) {
            selectedLand.Select(false);
        }

    //set the new selected land
    selectedLand = land;
    land.Select(true);
    }
    
    public void Interact() {
        
        //Check if player has a tool equipped
        bool toolEquipped = InventoryManager.Instance.SlotEquipped(InventoryBox.InventoryType.Tool);

        //selecting any lands
        if(selectedLand != null){
            selectedLand.Interact();
            return;
        }
        Debug.Log("[PlayerInteraction] Not on land");
    }

    //interacting with items
    public void ItemInteract() {

        //Check if looking at an interactable object
        if(selectedInteractable != null) {
            //pick up the item
            selectedInteractable.Pickup();
            //Hide tooltip after pickup
            UIManager.Instance.HideTooltip();
            selectedInteractable = null;
            return;
        }

        //Hand -> Inventory
        if(InventoryManager.Instance.SlotEquipped(InventoryBox.InventoryType.Tool)) {
            InventoryManager.Instance.HandToInventory(InventoryBox.InventoryType.Item);
            return;
        }

    }

}

