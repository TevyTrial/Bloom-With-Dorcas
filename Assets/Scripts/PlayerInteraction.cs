using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    PlayerController playerController;

    Land selectedLand = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1)) {
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
        //selecting any lands
        if(selectedLand != null){
            selectedLand.Interact();
            return;
        }
        Debug.Log("Not on any land");
    }
}

