using UnityEngine;

public class Land : MonoBehaviour, ITimeTracker
{
public enum LandState
    {
        Soil,
        Tilled,
        Watered
    }

    public LandState landstate;

    public Material soilMat, tilledMat, wateredMat;
    new Renderer renderer;

    public GameObject select;

    GameTimeStamp timeWatered; //time when the land was watered

    [Header("Crop Information")]
    public GameObject cropObject; //the crop planted on this land
    CropBehaviour cropPlanted = null; //crop planted

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get the renderer component and set the initial state to soil
        renderer = GetComponent<Renderer>();
        //initial state
        SwitchState(LandState.Soil);

        select.SetActive(false);

        //Register as a listener to time updates
        TimeManager.Instance.RegisterListener(this);
    }

    public void SwitchState(LandState newstatus) {
        //set land state and switch material
        landstate = newstatus;

        //decide which material to switch to
        Material materialToSwitch = soilMat;
        switch (newstatus) {
            case LandState.Soil:
                materialToSwitch = soilMat;
                break;
            case LandState.Tilled:
                materialToSwitch = tilledMat;
                break;
            case LandState.Watered:
                materialToSwitch = wateredMat;
                
                //record the time when watered
                timeWatered = TimeManager.Instance.GetGameTimeStamp();
                break;
        }
        renderer.material = materialToSwitch;
    }

    public void Select(bool isSelected) {
        select.SetActive(isSelected);
    }

    //when player presses the interact button
    public void Interact() {
        //interaction
        Debug.Log("Interacted with land");
        //Check player's equipped tool
        ItemData toolSlot = InventoryManager.Instance.equippedTool;

        //if no tool is equipped, return
        if(toolSlot == null) {
            Debug.Log("No tool equipped");
            return;
        }
        
        //Try casting the itemdata in the toolslot as equipment data
        EquipmentData equippedTool = toolSlot as EquipmentData;

        //Check if the equipped tool the type of equipment data
        if(equippedTool != null) {

            //get the equipment type of the tool
            EquipmentData.ToolType toolType = equippedTool.toolType;
            
            //Check the type of tool
            switch(toolType) {
                case EquipmentData.ToolType.Hoe:
                    //If the land is in soil state, till it
                    if(landstate == LandState.Soil) {
                        SwitchState(LandState.Tilled);
                    }
                    break;
                case EquipmentData.ToolType.WaterCan:
                    //If the land is in tilled state, water it
                    if(landstate == LandState.Tilled) {
                        SwitchState(LandState.Watered);
                    }
                    break;
                default:
                    Debug.Log("Equipped tool is not a hoe or watering can");
                    break;
            }
            return;
        }
        
        Debug.Log("No valid tool equipped");
        //Try casting the itemdata in the toolslot as seed data
        SeedData seedTool = toolSlot as SeedData;

        //Condition for the player to plant a seed
        //1. He is holding a seed
        //2. The land is in tilled/watered state
        //3. There is no crop currently planted
        if(seedTool != null && landstate != LandState.Soil && cropPlanted == null) {
        // Check if cropObject prefab is assigned
            if(cropObject == null) {
            Debug.LogError("cropObject prefab is not assigned!");
            return;
            }
    
            GameObject cropObj = Instantiate(cropObject, transform);
    
            // Use local position instead of world position
            cropObj.transform.localPosition = new Vector3(0.061f, 0.8f, 0.14f);
            cropObj.transform.localScale = new Vector3(0.3f, 1.4f, 0.3f);
    
            // Check if CropBehaviour component exists
            cropPlanted = cropObj.GetComponent<CropBehaviour>();
            if(cropPlanted == null) {
                Debug.LogError("CropBehaviour component not found on cropObject prefab!");
                Destroy(cropObj); // Clean up the instantiated object
                return;
            }
            cropPlanted.plant(seedTool);
        }
    
        
    }

    public void ClockUpdate(GameTimeStamp currentTime)
    {
        //If the land is watered, check if 4 hours have passed since it was watered
        if(landstate == LandState.Watered) {
            int hoursPassed = GameTimeStamp.CompareTimeStamps(timeWatered, currentTime);
            Debug.Log("Hours passed since watered: " + hoursPassed);

            //Grow the planted crop
            if(cropPlanted != null){
                cropPlanted.grow();
            }

            if(hoursPassed >= 24) {
                //if 24 or more hours have passed, revert to tilled state
                SwitchState(LandState.Tilled);
            }
        }
    }
}
