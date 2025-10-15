using UnityEngine;

public class CropBehaviour : MonoBehaviour
{
    //Information on what the crop will yield when harvested
    SeedData seedToGrow;

    [Header("Crop Growth")]
    public GameObject seed;
    private GameObject seedling;
    private GameObject mature;

    public enum GrowthStage { Seed, Seedling, Mature }
    public GrowthStage currentStage;

    //The growth points of the crop
    int growth;

    //How many growth points are needed to advance to the next stage
    int maxGrowth;

    //Initialisation for the crop 
    //Called when the seed is planted
    /*
    public void plant(SeedData seedToGrow) {
        //save the seed information
        this.seedToGrow = seedToGrow;

        //instantiate the seed object
        seedling = Instantiate(seedToGrow.Seedling, transform);

        //get the crop to yield information
        ItemData CropToYield = seedToGrow.CropToYield;

        //Instantiate the mature crop object
        mature = Instantiate(CropToYield.matureCropModel, transform);

        //Convert Days to growth points
        int hoursToGrow = GameTimeStamp.ConvertDaysToHours(seedToGrow.growTimeInDays);
        //Each growth point is 2 hours
        maxGrowth = GameTimeStamp.ConvertHoursToMinutes(hoursToGrow); 

        //Set the initial state to seed
        SwitchState(GrowthStage.Seed);
    }
    */
    public void plant(SeedData seedToGrow) {
    this.seedToGrow = seedToGrow;
    seedling = Instantiate(seedToGrow.Seedling, transform);
    ItemData CropToYield = seedToGrow.CropToYield;
    
    // Check if matureCropModel exists before instantiating
    if(CropToYield.matureCropModel != null) {
        mature = Instantiate(CropToYield.matureCropModel, transform);
        
        // Ensure mature object has InteractableObject component
        InteractableObject interactable = mature.GetComponent<InteractableObject>();
        if(interactable == null) {
            interactable = mature.AddComponent<InteractableObject>();
        }
        interactable.item = CropToYield;
        

    }
    
    int hoursToGrow = GameTimeStamp.ConvertDaysToHours(seedToGrow.growTimeInDays);
    maxGrowth = GameTimeStamp.ConvertHoursToMinutes(hoursToGrow);
    SwitchState(GrowthStage.Seed);
}

    public void grow() {
        //Increase growth points
        growth++;

        //Check if we need to advance to the next stage
        if (growth >= maxGrowth / 2 && currentStage == GrowthStage.Seed) {
            //Advance to seedling stage
            SwitchState(GrowthStage.Seedling);
        } else if (growth >= maxGrowth && currentStage == GrowthStage.Seedling) {
            //Advance to mature stage
            SwitchState(GrowthStage.Mature);
        }

    }

    //Function to handle the state change of the crop
    private void SwitchState(GrowthStage stateToSwitch) {
        //Reset everything and set all to inactive
        seed.SetActive(false);
        seedling.SetActive(false);
        mature.SetActive(false);

        //Handle the visual representation of each state
        switch (stateToSwitch) {
            //Enable the seed object
            case GrowthStage.Seed:
                seed.SetActive(true);
                break;
            //Enable the seedling object
            case GrowthStage.Seedling:
                seedling.SetActive(true);
                break;
            //Enable the mature object
            case GrowthStage.Mature:
                mature.SetActive(true);
                //Unparent it to crop
                mature.transform.parent = null;
                //Destroy the crop object
                Destroy(gameObject);
                break;
        }

        //Set the current stage to the new state
        currentStage = stateToSwitch;
    }

}
