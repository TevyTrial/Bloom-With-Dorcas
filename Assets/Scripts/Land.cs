using UnityEngine;

public class Land : MonoBehaviour
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get the renderer component and set the initial state to soil
        renderer = GetComponent<Renderer>();
        //initial state
        SwitchState(LandState.Soil);

        select.SetActive(false);
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
        SwitchState(LandState.Tilled);
    }
}
