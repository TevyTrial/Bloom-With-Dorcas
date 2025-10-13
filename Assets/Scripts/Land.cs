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
        }
        else {
            Debug.Log("No tool equipped");
        }

    }

    public void ClockUpdate(GameTimeStamp currentTime)
    {
        //If the land is watered, check if 4 hours have passed since it was watered
        if(landstate == LandState.Watered) {
            int hoursPassed = GameTimeStamp.CompareTimeStamps(timeWatered, currentTime);
            if(hoursPassed >= 24) {
                //if 24 or more hours have passed, revert to tilled state
                SwitchState(LandState.Tilled);
            }
        }
    }
}
