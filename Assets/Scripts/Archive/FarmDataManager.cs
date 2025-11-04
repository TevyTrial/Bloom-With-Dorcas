using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent manager that handles farm data across all scenes.
/// Processes crop growth and land state changes even when not in the Garden scene.
/// </summary>
public class FarmDataManager : MonoBehaviour, ITimeTracker
{
    public static FarmDataManager Instance { get; private set; }

    // Persistent farm data
    [SerializeField] private List<LandSaveState> landData = new List<LandSaveState>();
    [SerializeField] private List<CropSaveState> cropData = new List<CropSaveState>();

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Register with TimeManager after scene is fully loaded (use Start instead of OnEnable)
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.RegisterListener(this);
        }
    }

    private void OnDestroy()
    {
        // Deregister from TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.UnregisterListener(this);
        }
        
        // Clear instance if this is being destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Called every in-game minute by TimeManager
    public void ClockUpdate(GameTimeStamp currentTime)
    {
        ProcessCropTimeUpdates();
        ProcessLandTimeUpdates(currentTime);
    }

    // Process crop growth and withering based on land state
    private void ProcessCropTimeUpdates()
    {
        for (int i = 0; i < cropData.Count; i++)
        {
            CropSaveState crop = cropData[i];
            
            // Get the land state for this crop by landID
            if (crop.landID >= 0 && crop.landID < landData.Count)
            {
                LandSaveState land = landData[crop.landID];
                
                // If land is watered, grow the crop
                if (land.landStatus == Land.LandState.Watered)
                {
                    cropData[i] = crop.Grow();
                }
                // If land is not watered (tilled or soil), wither the crop
                else if (land.landStatus == Land.LandState.Tilled)
                {
                    cropData[i] = crop.Wither();
                }
            }
        }
    }

    // Process land state changes (watered -> tilled after 24 hours)
    private void ProcessLandTimeUpdates(GameTimeStamp currentTime)
    {
        for (int i = 0; i < landData.Count; i++)
        {
            LandSaveState land = landData[i];
            
            if (land.landStatus == Land.LandState.Watered)
            {
                // Check if 24 hours have passed since last watered
                int hoursPassed = GameTimeStamp.CompareTimeStamps(land.lastWatered, currentTime);
                
                if (hoursPassed >= 24)
                {
                    // Change watered land back to tilled
                    landData[i] = new LandSaveState(
                        Land.LandState.Tilled,
                        land.lastWatered
                    );
                }
            }
        }
    }

    // Get all farm data (for LandManager when entering Garden scene)
    public Tuple<List<LandSaveState>, List<CropSaveState>> GetFarmData()
    {
        return new Tuple<List<LandSaveState>, List<CropSaveState>>(
            new List<LandSaveState>(landData),
            new List<CropSaveState>(cropData)
        );
    }

    // Update farm data (called by LandManager when leaving Garden scene)
    public void UpdateFarmData(List<LandSaveState> newLandData, List<CropSaveState> newCropData)
    {
        landData = new List<LandSaveState>(newLandData);
        cropData = new List<CropSaveState>(newCropData);
    }

    // Update a single land state
    public void UpdateLandState(int landID, Land.LandState landStatus, GameTimeStamp lastWatered)
    {
        if (landID >= 0 && landID < landData.Count)
        {
            landData[landID] = new LandSaveState(landStatus, lastWatered);
        }
    }

    // Update a single crop state
    public void UpdateCropState(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented, Vector3 maturePosition, Quaternion matureRotation)
    {
        int cropIndex = cropData.FindIndex(x => x.landID == landID);
        
        if (cropIndex != -1)
        {
            cropData[cropIndex] = new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, maturePosition, matureRotation);
        }
        else
        {
            cropData.Add(new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, maturePosition, matureRotation));
        }
    }

    // Register a new crop
    public void RegisterCrop(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented)
    {
        cropData.Add(new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, Vector3.zero, Quaternion.identity));
    }

    // Remove a crop (when harvested)
    public void DeregisterCrop(int landID)
    {
        cropData.RemoveAll(x => x.landID == landID);
    }

    // Initialize land data (called by LandManager on first load)
    public void InitializeLandData(int plotCount)
    {
        if (landData.Count == 0)
        {
            // Use current game time from TimeManager
            GameTimeStamp currentTime = TimeManager.Instance != null 
                ? TimeManager.Instance.GetGameTimeStamp() 
                : new GameTimeStamp(1, GameTimeStamp.Season.Spring, 1, 6, 0);
            
            for (int i = 0; i < plotCount; i++)
            {
                landData.Add(new LandSaveState(Land.LandState.Soil, currentTime));
            }
        }
    }
}
