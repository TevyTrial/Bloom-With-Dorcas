using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;

    private bool isBusy = false;

    private Land selectedLand = null;
    private InteractableObject selectedInteractable = null;
    private DialogueScript selectedNPC = null;

    [Header("Interactables")]
    [SerializeField] private float interactableDetectionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayerMask = ~0;
    [SerializeField] private float interactableForwardThreshold = -0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }
        if (animator == null)
        {
            animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        }
    }

// Update is called once per frame
void Update()
{
    if(UIManager.Instance.shopPanel.activeSelf) {
        ClearSelectedInteractable();
        return;
    }

    UpdateInteractableSelection();

    if(selectedInteractable != null && Input.GetKeyDown(KeyCode.F)) {
        ItemInteract();
    }
    
    RaycastHit hit;
    
    // Check forward raycast for land, then downward as fallback
    if (Physics.Raycast(transform.position, transform.forward, out hit, 3f) ||
        Physics.Raycast(transform.position, Vector3.down, out hit, 2f)) {
        OnInteractWithObject(hit);
    }
    else {
        // Clear land and NPC selections when nothing detected
        if(selectedNPC != null) {
            selectedNPC = null;
            UIManager.Instance.HideInteractTooltip();
        }
        if(selectedLand != null) {
            selectedLand.Select(false);
            selectedLand = null;
        }
    }
    
    // Start NPC conversation with T key
    if (selectedNPC != null && Input.GetKeyDown(KeyCode.T)) {
        selectedNPC.StartConversation();
    }
}


void OnInteractWithObject(RaycastHit hit) {
    Collider collider = hit.collider;

    if (collider.CompareTag("Land")) {
        Land land = collider.GetComponent<Land>();
        SelectLand(land);
        return;
    }

   if (collider.CompareTag("NPC")) {
       selectedNPC = collider.GetComponent<DialogueScript>();
       if (selectedNPC != null && !selectedNPC.IsInConversation()) {
           UIManager.Instance.ShowInteractTooltip();
       }
       return;
   }

   if (selectedNPC != null) {
       selectedNPC = null;
       UIManager.Instance.HideInteractTooltip();
   }

   if (selectedLand != null) {
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
        
        // Don't allow interaction while performing an action
        if(isBusy) {
            return;
        }
        
        //selecting any lands
        if(selectedLand != null){
            //Check if player has a tool or seed equipped in the tool slot
            bool toolEquipped = InventoryManager.Instance.SlotEquipped(InventoryBox.InventoryType.Tool);
            
            if(!toolEquipped) {
                Debug.Log("No tool or seed equipped");
                return;
            }
            
            //Get the equipped item data (could be a tool or a seed)
            ItemData toolSlot = InventoryManager.Instance.GetEquippedItemSlots(InventoryBox.InventoryType.Tool);
            
            //Try casting as equipment data (tools)
            EquipmentData equipmentTool = toolSlot as EquipmentData;
            if(equipmentTool != null) {
                //Get the tool type and trigger animation
                EquipmentData.ToolType toolType = equipmentTool.toolType;

                switch (toolType) {
                    case EquipmentData.ToolType.Hoe:
                        StartCoroutine(PlayAnimationAndInteract("Plowing", 1.5f, AudioManager.Instance.PlayPlowingSFX));
                        break;
                    case EquipmentData.ToolType.WaterCan:
                        StartCoroutine(PlayAnimationAndInteract("Watering", 1.5f, AudioManager.Instance.PlayWateringSFX));
                        break;
                    case EquipmentData.ToolType.Rake:
                        StartCoroutine(PlayAnimationAndInteract("Raking", 1f, AudioManager.Instance.PlayRakingSFX));
                        break;
                    default:
                        Debug.Log("Equipped tool is not handled");
                        break;
                }
                
                return;
            }

            // planting should happen immediately (no tool animation)
            SeedData seed = toolSlot as SeedData;
            if (seed != null) {
                // Directly interact with the land to plant the seed
                selectedLand.Interact();
                return;
            }
            return;
        }
    }

    private IEnumerator PlayAnimationAndInteract(string triggerName, float duration, System.Action playSfx)
    {
        if (isBusy) yield break;
        isBusy = true;

        DisableMovement();

        animator.SetTrigger(triggerName);
        playSfx?.Invoke();

        yield return new WaitForSeconds(duration);

        if(selectedLand != null) {
            selectedLand.Interact();
        }

        EnableMovement();
        isBusy = false;
    }

    private void DisableMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            var controller = playerController.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
        }
    }

    private void EnableMovement()
    {
        if (playerController != null)
        {
            var controller = playerController.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = true;
            playerController.enabled = true;
        }
    }

    void UpdateInteractableSelection() {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactableDetectionRadius, interactableLayerMask, QueryTriggerInteraction.Collide);

        InteractableObject bestCandidate = null;
        float bestScore = float.NegativeInfinity;
        HashSet<InteractableObject> processed = new HashSet<InteractableObject>();

        foreach(Collider hit in hits) {
            if(hit == null) {
                continue;
            }

            InteractableObject candidate = hit.GetComponentInParent<InteractableObject>();
            if(candidate == null) {
                continue;
            }

            if(!processed.Add(candidate)) {
                continue;
            }

            if(candidate.item == null) {
                continue;
            }

            Vector3 toCandidate = candidate.transform.position - transform.position;
            float distance = toCandidate.magnitude;
            if(distance <= Mathf.Epsilon) {
                distance = 0.001f;
            }

            Vector3 direction = toCandidate.normalized;
            float forwardDot = Vector3.Dot(transform.forward, direction);
            if(forwardDot < interactableForwardThreshold) {
                continue;
            }

            float score = forwardDot * 2f - distance;
            if(score > bestScore) {
                bestScore = score;
                bestCandidate = candidate;
            }
        }

        if(bestCandidate != selectedInteractable) {
            ClearSelectedInteractable();

            if(bestCandidate != null) {
                selectedInteractable = bestCandidate;
                UIManager.Instance.ShowHarvestTooltip();
            }
        }

        if(selectedInteractable != null && selectedInteractable.item == null) {
            ClearSelectedInteractable();
        }
    }

    void ClearSelectedInteractable() {
        if(selectedInteractable == null) {
            return;
        }

        UIManager.Instance.HideHarvestTooltip();
        selectedInteractable = null;
    }

    public void ItemInteract() {
        if(selectedInteractable == null) {
            Debug.Log("ItemInteract called but no interactable selected.");
            return;
        }

        InteractableObject interactable = selectedInteractable;
        ClearSelectedInteractable();
        interactable.Pickup();
    }

}