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

    [Header("Instrument Audio")]
    private AudioSource instrumentSource;
    private InstrumentTrack assignedInstrument;
    private bool hasMatureUnparented = false; // Track if we've unparented the mature crop

    public void plant(SeedData seedToGrow) {
        this.seedToGrow = seedToGrow;
        
        // Validate seed data
        if(seedToGrow.growthStageModels == null || seedToGrow.growthStageModels.Length == 0) {
            Debug.LogError($"Seed {seedToGrow.name} has no growth stage models defined!");
            return;
        }
        
        // Reset growth state
        growth = 0;
        health = maxHealth;
        currentStageIndex = 0;
        isWilted = false;
        hasMatureUnparented = false;
        
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
        if (isWilted || hasMatureUnparented) return; // Do not grow if wilted or already matured

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
        // Don't wilt if it have not germenated yet or already matured
        if (currentStageIndex == 0 || hasMatureUnparented) return;
        
        health--;
        if (health <= 0 && !isWilted)
        {
            isWilted = true;
            StopInstrument(); // Stop playing music when wilted
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
            // Final stage: Mature
            int matureIndex = stageObjects.Length - 1;
            if (stageObjects[matureIndex] != null)
            {
                stageObjects[matureIndex].SetActive(true);
                stageObjects[matureIndex].transform.parent = null;

                // Start playing instrument when crop matures
                StartInstrument();

                // Add CropInstrumentController to the mature crop so it can stop music when harvested
                CropInstrumentController controller = stageObjects[matureIndex].AddComponent<CropInstrumentController>();
                controller.Initialize(instrumentSource);

                // Reset health on mature crop
                health = maxHealth;
                
                // Mark that we've unparented the mature crop
                hasMatureUnparented = true;
                
                // Destroy this CropBehaviour GameObject after a small delay
                // This allows the tile to be replanted
                Destroy(gameObject);
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

    // Start playing the crop's instrument track
    private void StartInstrument()
    {
        if (AudioManager.Instance == null) return;

        // Get instrument track (specific or random)
        assignedInstrument = seedToGrow.specificInstrument ?? AudioManager.Instance.GetRandomInstrumentTrack();

        if (assignedInstrument != null && assignedInstrument.audioClip != null)
        {
            instrumentSource = AudioManager.Instance.RegisterCropInstrument(assignedInstrument);
        }
    }

    // Stop playing the crop's instrument track
    private void StopInstrument()
    {
        if (AudioManager.Instance != null && instrumentSource != null)
        {
            AudioManager.Instance.UnregisterCropInstrument(instrumentSource);
            instrumentSource = null;
        }
    }

    private void OnDestroy()
    {
        // Only clean up instrument if we haven't unparented (wilted crops, manual destroy)
        // If we've unparented, the CropInstrumentController handles it
        if (!hasMatureUnparented)
        {
            StopInstrument();
        }
    }
}