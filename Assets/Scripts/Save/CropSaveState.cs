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
}