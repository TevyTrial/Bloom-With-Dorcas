using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    PlayerController playerController;

    Land selectedLand = null;

    //The interactable object the player is currently looking at
    InteractableObject selectedInteractable = null;

    //The NPC the player is currently looking at
    DialogueScript selectedNPC = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(UIManager.Instance.shopPanel.activeSelf) {
            //Don't allow interaction while shop is open
            return;
        }
        RaycastHit hit;
        // Raycast forward from player position to detect objects in front
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f)) {
            OnInteractWithObject(hit);                    
        }
        // Also keep downward raycast for land detection
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, 1)) {
            OnInteractWithObject(hit);                    
        }
        // Start NPC conversation with T key
        if (selectedNPC != null && Input.GetKeyDown(KeyCode.T)) {
            selectedNPC.StartConversation();
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

            if (selectedInteractable == null) {
                selectedInteractable = collider.GetComponentInParent<InteractableObject>();
            }
            
            if(selectedInteractable == null) {
                selectedInteractable = collider.GetComponentInChildren<InteractableObject>();
            }
            
            //Show tooltip for the item
            if(selectedInteractable != null && selectedInteractable.item != null) {
                UIManager.Instance.ShowHarvestTooltip();
            } else {
                UIManager.Instance.HideHarvestTooltip();
            }

            return;
        }

        // Check if looking at an NPC
        if(collider.tag == "NPC") {
            selectedNPC = collider.GetComponent<DialogueScript>();

            if(selectedNPC != null) {
                // Only show interact tooltip if not in conversation
                if(!selectedNPC.IsInConversation()) {
                    UIManager.Instance.ShowInteractTooltip();
                }
            }
            return;
        }

        //Deselect the old interactable
        if(selectedInteractable != null) {
            selectedInteractable = null;
            UIManager.Instance.HideHarvestTooltip();
        }

        //Deselect the old NPC
        if(selectedNPC != null) {
            selectedNPC = null;
            UIManager.Instance.HideInteractTooltip();
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
    }

    //interacting with items
    public void ItemInteract() {

        //Check if looking at an interactable object
        if(selectedInteractable != null) {
            //pick up the item
            selectedInteractable.Pickup();
            //Hide tooltip after pickup
            UIManager.Instance.HideHarvestTooltip();
            selectedInteractable = null;
            return;
        }

    }

    public void ItemKeep() {
        //Hand -> Inventory
        if(InventoryManager.Instance.SlotEquipped(InventoryBox.InventoryType.Tool)) {
            InventoryManager.Instance.HandToInventory(InventoryBox.InventoryType.Item);
            return;
        }
    }

}