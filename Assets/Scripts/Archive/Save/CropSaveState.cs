using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CropSaveState
{
    public int landID;
    public string seedToGrowName; // Name or ID of the SeedData
    public int currentStageIndex;
    public int growth;
    public int health;
    public bool isWilted;
    public bool hasMatureUnparented;
    
    // Position and rotation for mature unparented crops
    public Vector3 maturePosition;
    public Quaternion matureRotation;

    public CropSaveState(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented, Vector3 maturePosition = default, Quaternion matureRotation = default)
    {
        this.landID = landID;
        this.seedToGrowName = seedToGrowName;
        this.currentStageIndex = currentStageIndex;
        this.growth = growth;
        this.health = health;
        this.isWilted = isWilted;
        this.hasMatureUnparented = hasMatureUnparented;
        this.maturePosition = maturePosition;
        this.matureRotation = matureRotation;
    }

    public CropSaveState Grow()
    {
        if (isWilted || hasMatureUnparented) return this; // Return unchanged

        //Increase growth points
        int newGrowth = growth + 1;

        // Convert the seedToGrowName string into SeedData
        // Guard against null InventoryManager during scene loading
        if (InventoryManager.Instance == null || InventoryManager.Instance.itemIndex == null)
        {
            return this; // Return unchanged if managers not ready
        }
        
        SeedData seedInfo = (SeedData) InventoryManager.Instance.itemIndex.GetItemFromString(seedToGrowName);
        
        if (seedInfo == null || seedInfo.growthStageModels == null || seedInfo.growthStageModels.Length == 0)
        {
            return this; // Return unchanged silently (avoid Debug calls during loading)
        }

        // Calculate total stages: seed (1) + growth stage models
        int totalStages = seedInfo.growthStageModels.Length + 1;
        
        // Get the max health and growth from the seed Data
        int maxGrowth = GameTimeStamp.ConvertHoursToMinutes(GameTimeStamp.ConvertDaysToHours(seedInfo.growTimeInDays));
        int maxHealth = GameTimeStamp.ConvertHoursToMinutes(36); // 1.5 days

        // Calculate which stage we should be at based on growth progress
        // Divide growth period evenly among all stages
        float growthPerStage = (float)maxGrowth / (totalStages - 1);
        int targetStage = Mathf.Min(Mathf.FloorToInt(newGrowth / growthPerStage), totalStages - 1);

        // Handle health recovery
        int newHealth = health < maxHealth ? health + 1 : health;

        // Advance stage if needed
        int newStageIndex = currentStageIndex;
        bool newHasMatureUnparented = hasMatureUnparented;
        
        if (targetStage > currentStageIndex)
        {
            newStageIndex = targetStage;
            
            // If reached mature stage, mark as unparented
            if (newStageIndex == totalStages - 1)
            {
                newHasMatureUnparented = true;
            }
        }
        
        // Return a new modified struct
        return new CropSaveState(
            landID,
            seedToGrowName,
            newStageIndex,
            newGrowth,
            newHealth,
            isWilted,
            newHasMatureUnparented,
            maturePosition,
            matureRotation
        );
    }

    public CropSaveState Wither()
    {
        // Don't wilt if it has not germinated yet or already matured
        if (currentStageIndex == 0 || hasMatureUnparented) return this;
        
        int newHealth = health - 1;
        bool newIsWilted = isWilted;
        
        if (newHealth <= 0 && !isWilted)
        {
            newIsWilted = true;
        }

        // Return a new modified struct
        return new CropSaveState(
            landID,
            seedToGrowName,
            currentStageIndex,
            growth,
            newHealth,
            newIsWilted,
            hasMatureUnparented,
            maturePosition,
            matureRotation
        );
    }
}