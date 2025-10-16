using UnityEngine;

public class CropBehaviour : MonoBehaviour
{
    
    //Information on what the crop will yield when harvested
    SeedData seedToGrow;

    [Header("Crop Growth")]
    public GameObject seed; // The planted seed visual (stage 0)
    private GameObject[] stageObjects; // Instantiated stage models
    private GameObject wiltedModel;

    //Current growth stage index (0 = seed, 1+ = stages from SeedData)
    private int currentStageIndex = 0;
    
    //The growth points of the crop
    int growth;
    //How many growth points are needed to advance to the next stage
    int maxGrowth;

    //How many total stages this crop has
    private int totalStages;

    //Health system for the crop (for wilting)
    private int health;
    private int maxHealth = GameTimeStamp.ConvertHoursToMinutes(36); // 1.5 days in minutes
    private bool isWilted = false;


    public void plant(SeedData seedToGrow) {
        this.seedToGrow = seedToGrow;
        
        // Validate seed data
        if(seedToGrow.growthStageModels == null || seedToGrow.growthStageModels.Length == 0) {
            Debug.LogError($"Seed {seedToGrow.name} has no growth stage models defined!");
            return;
        }
        
        // Total stages = seed (always present) + defined models
        totalStages = seedToGrow.growthStageModels.Length + 1;
        
        // Instantiate all stage models except the last one (mature)
        stageObjects = new GameObject[seedToGrow.growthStageModels.Length];
        for(int i = 0; i < seedToGrow.growthStageModels.Length - 1; i++) {
            if(seedToGrow.growthStageModels[i] != null) {
                stageObjects[i] = Instantiate(seedToGrow.growthStageModels[i], transform);
                stageObjects[i].SetActive(false);
            }
        }
        
        // Handle mature stage (last model) - needs InteractableObject
        int matureIndex = seedToGrow.growthStageModels.Length - 1;
        if (seedToGrow.growthStageModels[matureIndex] != null)
        {
            stageObjects[matureIndex] = Instantiate(seedToGrow.growthStageModels[matureIndex], transform);
            stageObjects[matureIndex].SetActive(false);

            // Add InteractableObject to mature crop
            InteractableObject interactable = stageObjects[matureIndex].GetComponent<InteractableObject>();
            if (interactable == null)
            {
                interactable = stageObjects[matureIndex].AddComponent<InteractableObject>();
            }
            interactable.item = seedToGrow.CropToYield;
        }

        // Instantiate wilted model if defined
        if (seedToGrow.WiltedModel != null)
        {
            wiltedModel = Instantiate(seedToGrow.WiltedModel, transform);
            wiltedModel.SetActive(false);
        }
        
        // Calculate growth time
        int hoursToGrow = GameTimeStamp.ConvertDaysToHours(seedToGrow.growTimeInDays);
        maxGrowth = GameTimeStamp.ConvertHoursToMinutes(hoursToGrow);
        
        // Start at seed stage
        SwitchToStage(0);
    }

    public void grow()
    {
        if (isWilted) return; // Do not grow if wilted

        //Increase growth points
        growth++;

        // Calculate which stage we should be at based on growth progress
        // Divide growth period evenly among all stages
        float growthPerStage = (float)maxGrowth / (totalStages - 1);
        int targetStage = Mathf.Min(Mathf.FloorToInt(growth / growthPerStage), totalStages - 1);

        // Handle wilting if health depletes
        if (health < maxHealth)
        {
            health++;
        }

        // Advance stage if needed
        if (targetStage > currentStageIndex)
        {
            SwitchToStage(targetStage);
        }
    }

    public void wilted()
    {
        // Don't wilt if it have not germenated yet
        if (currentStageIndex == 0) return;
        
        health--;
        if (health <= 0 && !isWilted)
        {
            isWilted = true;
            SwitchToStage(-1);
        }
        Debug.Log($"Crop {seedToGrow.name} health: {health}/{maxHealth}");
    }

    //Function to handle the state change of the crop
    private void SwitchToStage(int stageIndex) {
        // Deactivate seed
        seed.SetActive(false);

        // Deactivate all stage objects
        if (stageObjects != null)
        {
            foreach (var obj in stageObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // Deactivate wilted model
        if (wiltedModel != null)
        {
            wiltedModel.SetActive(false);
        }

        // If stageIndex is -1, show wilted model
        if (stageIndex == -1)
        {
            if (wiltedModel != null)
            {
                wiltedModel.SetActive(true);
            }
            return;
        }

        // Activate appropriate stage
        if (stageIndex == 0)
        {
            // Stage 0: Seed
            seed.SetActive(true);
        }
        else if (stageIndex == totalStages - 1)
        {
            // Final stage: Mature (unparent and destroy crop behavior)
            int matureIndex = stageObjects.Length - 1;
            if (stageObjects[matureIndex] != null)
            {
                stageObjects[matureIndex].SetActive(true);
                stageObjects[matureIndex].transform.parent = null;
                Destroy(gameObject);
                // Reset health on mature crop
                health = maxHealth;
            }
        }
        else
        {
            // Intermediate stages
            int modelIndex = stageIndex - 1;
            if (modelIndex < stageObjects.Length && stageObjects[modelIndex] != null)
            {
                stageObjects[modelIndex].SetActive(true);
            }
        }

        

        currentStageIndex = stageIndex;

        
    }

}
/*
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
    private void SwitchState(GrowthStage stateToSwitch)
    {
        //Reset everything and set all to inactive
        seed.SetActive(false);
        seedling.SetActive(false);
        mature.SetActive(false);

        //Handle the visual representation of each state
        switch (stateToSwitch)
        {
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
*/