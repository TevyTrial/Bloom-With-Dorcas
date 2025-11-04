using System.Collections.Generic;
using UnityEngine;

public class LandManager : MonoBehaviour
{
    public static LandManager Instance { get; private set; } 

    [SerializeField] private List<Land> landPlots = new List<Land>();

    private void Awake()
    {
        //If there is more than one instance, destroy the extra
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            //Set the static instance to this instance
            Instance = this;
        }
    }

    private void Start()
    {
        RegisterLandPlots();
    }

    void RegisterLandPlots()
    {
        landPlots.Clear();

        foreach(Transform landTransform in transform)
        {
            Land land = landTransform.GetComponent<Land>();
            if (land == null)
            {
                continue;
            }

            landPlots.Add(land);

            // Assign an ID based on index
            land.id = landPlots.Count - 1;
        }
    }

    public Land GetLandPlot(int landID)
    {
        if (landID >= 0 && landID < landPlots.Count)
        {
            return landPlots[landID];
        }
        return null;
    }

    public int GetLandPlotCount()
    {
        return landPlots.Count;
    }
}

/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandManager : MonoBehaviour
{
    public static LandManager Instance { get; private set; } 

    [SerializeField] private List<Land> landPlots = new List<Land>();   
    
    // Local references to farm data (synced with FarmDataManager)
    [SerializeField] private List<LandSaveState> landData = new List<LandSaveState>();
    [SerializeField] private List<CropSaveState> cropData = new List<CropSaveState>();

    private bool hasLoadedData = false; // Track if we've loaded data

    private void Awake()
    {
        //If there is more than one instance, destroy the extra
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            //Set the static instance to this instance
            Instance = this;
        }
    }

    void OnEnable()
    {
        RegisterLandPlots();
        StartCoroutine(LoadFarmData());
    }

    private IEnumerator LoadFarmData()
    {
        yield return new WaitForEndOfFrame();

        // Get farm data from persistent FarmDataManager
        if (FarmDataManager.Instance != null)
        {
            var farmData = FarmDataManager.Instance.GetFarmData();
            if (farmData != null)
            {
                ImportFarmData(farmData.Item1);
                ImportCropData(farmData.Item2);
                hasLoadedData = true;
            }
        }
    }

    private void OnDestroy()
    {
        // Save unparented mature crops before updating FarmDataManager
        SaveMatureCrops();
        
        // Update the persistent FarmDataManager with current state
        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.UpdateFarmData(landData, cropData);
        }

        hasLoadedData = false;
    }

    private void SaveMatureCrops()
    {
        // Find all MatureCropTracker components in the scene
        MatureCropTracker[] matureCrops = FindObjectsOfType<MatureCropTracker>();

        foreach (MatureCropTracker crop in matureCrops) {
            // Check if the crop is already in cropdata
            int existingIndex = cropData.FindIndex(x => x.landID == crop.landID);
            if (existingIndex == -1) {
                //Add the mature crop to cropdata
                SeedData seedData = (SeedData) InventoryManager.Instance.itemIndex.GetItemFromString(crop.seedName);
                if (seedData != null) {
                    int totalStages = seedData.growthStageModels.Length + 1;
                    cropData.Add(new CropSaveState(
                        crop.landID, 
                        crop.seedName, 
                        totalStages - 1,
                        0, 
                        0, 
                        false, 
                        true,
                        crop.transform.position,
                        crop.transform.rotation
                    ));
                }
            }
            else
            {
                // Update existing entry with current position/rotation
                CropSaveState existing = cropData[existingIndex];
                cropData[existingIndex] = new CropSaveState(
                    existing.landID,
                    existing.seedToGrowName,
                    existing.currentStageIndex,
                    existing.growth,
                    existing.health,
                    existing.isWilted,
                    existing.hasMatureUnparented,
                    crop.transform.position,
                    crop.transform.rotation
                );
            }
        }
    }

    void RegisterLandPlots()
    {
        landPlots.Clear();

        foreach(Transform landTransform in transform)
        {
            Land land = landTransform.GetComponent<Land>();
            if (land == null)
            {
                continue;
            }

            landPlots.Add(land);

            // Assign an ID based on index
            land.id = landPlots.Count - 1;
        }
        
        // Initialize FarmDataManager if needed (only on first time)
        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.InitializeLandData(landPlots.Count);
        }
        
        // DON'T initialize landData here - wait for ImportFarmData to load it
        // Only initialize if we have no data at all (this shouldn't happen if FarmDataManager works)
        if (landData.Count == 0 && FarmDataManager.Instance == null)
        {
            // Emergency fallback only
            GameTimeStamp defaultTime = TimeManager.Instance != null 
                ? new GameTimeStamp(TimeManager.Instance.GetGameTimeStamp()) 
                : new GameTimeStamp(1, GameTimeStamp.Season.Spring, 1, 6, 0);
                
            for (int i = 0; i < landPlots.Count; i++)
            {
                landData.Add(new LandSaveState(Land.LandState.Soil, defaultTime));
            }
        }
    }

    // Registers the crop onto the FarmDataManager
    public void RegisterCrop(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented)
    {
        CropSaveState newState = new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, Vector3.zero, Quaternion.identity);

        int existingIndex = cropData.FindIndex(x => x.landID == landID);
        if (existingIndex != -1)
        {
            cropData[existingIndex] = newState;
        }
        else
        {
            cropData.Add(newState);
        }

        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.UpdateCropState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, Vector3.zero, Quaternion.identity);
        }
    }

    public void DeregisterCrop(int landID)
    {
        cropData.RemoveAll(x => x.landID == landID);
        
        // Also update FarmDataManager
        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.DeregisterCrop(landID);
        }
    }

    // Update the land data
    public void OnLandStateChanged(int id, Land.LandState landStatus, GameTimeStamp lastWatered)
    {
        if (id < 0)
        {
            return;
        }

        while (landData.Count <= id)
        {
            GameTimeStamp defaultTime = TimeManager.Instance != null
                ? new GameTimeStamp(TimeManager.Instance.GetGameTimeStamp())
                : new GameTimeStamp(1, GameTimeStamp.Season.Spring, 1, 6, 0);
            landData.Add(new LandSaveState(Land.LandState.Soil, defaultTime));
        }

        landData[id] = new LandSaveState(landStatus, lastWatered);
        
        // Also update FarmDataManager
        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.UpdateLandState(id, landStatus, lastWatered);
        }
    }

    // Update the crop data
    public void OnCropStateChanged(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented)
    {
        // Find its index in the list
        int cropIndex = cropData.FindIndex(x => x.landID == landID);
        
        Vector3 savedPosition = Vector3.zero;
        Quaternion savedRotation = Quaternion.identity;

        if (cropIndex != -1)
        {
            savedPosition = cropData[cropIndex].maturePosition;
            savedRotation = cropData[cropIndex].matureRotation;
            cropData[cropIndex] = new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, savedPosition, savedRotation);
        }
        else
        {
            CropSaveState newCrop = new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, savedPosition, savedRotation);
            cropData.Add(newCrop);
        }

        if (FarmDataManager.Instance != null)
        {
            FarmDataManager.Instance.UpdateCropState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, savedPosition, savedRotation);
        }
    }

    //Load over the static farmData onto the instance's landdata
    public void ImportFarmData(List<LandSaveState> landDatasetToLoad)
    {
        // Clear and resize landData to match the number of plots
        landData.Clear();
        
        for(int i = 0; i < landPlots.Count; i++)
        {
            if (i < landDatasetToLoad.Count)
            {
                // Load saved data
                LandSaveState landStateToLoad = landDatasetToLoad[i];
                landPlots[i].LoadLandData(landStateToLoad.landStatus, landStateToLoad.lastWatered);
                landData.Add(landStateToLoad);
            }
            else
            {
                // Initialize new plots that didn't exist before
                GameTimeStamp defaultTime = TimeManager.Instance != null 
                    ? new GameTimeStamp(TimeManager.Instance.GetGameTimeStamp()) 
                    : new GameTimeStamp(1, GameTimeStamp.Season.Spring, 1, 6, 0);
                LandSaveState defaultState = new LandSaveState(Land.LandState.Soil, defaultTime);
                landPlots[i].LoadLandData(defaultState.landStatus, defaultState.lastWatered);
                landData.Add(defaultState);
            }
        }
    }

    // Load crop data onto the instance's cropdata
    public void ImportCropData(List<CropSaveState> cropDatasetToLoad)
    {
        cropData = new List<CropSaveState>(cropDatasetToLoad);
        foreach(CropSaveState cropSave in cropDatasetToLoad)
        {
            // Handle mature unparented crops
            if (cropSave.hasMatureUnparented)
            {
                if (cropSave.landID < 0 || cropSave.landID >= landPlots.Count)
                {
                    Debug.LogWarning($"Skipping mature crop restoration for landID {cropSave.landID}: no matching land plot found.");
                    continue;
                }

                // Spawn the mature crop at its saved position
                SeedData matureSeedData = (SeedData)InventoryManager.Instance.itemIndex.GetItemFromString(cropSave.seedToGrowName);
                if (matureSeedData != null && matureSeedData.growthStageModels != null && matureSeedData.growthStageModels.Length > 0)
                {
                    int matureIndex = matureSeedData.growthStageModels.Length - 1;
                    GameObject matureCrop = Instantiate(matureSeedData.growthStageModels[matureIndex]);
                    
                    // Use saved position/rotation if available, otherwise calculate from land
                    if (cropSave.maturePosition != Vector3.zero)
                    {
                        matureCrop.transform.position = cropSave.maturePosition;
                        matureCrop.transform.rotation = cropSave.matureRotation;
                    }
                    else
                    {
                        // Fallback to land position
                        Land matureLand = landPlots[cropSave.landID];
                        matureCrop.transform.position = matureLand.transform.position + new Vector3(0.061f, 0.8f, 0.14f);
                        matureCrop.transform.localScale = new Vector3(0.3f, 1.4f, 0.3f);
                    }

                    // Add InteractableObject
                    InteractableObject interactable = matureCrop.GetComponent<InteractableObject>();
                    if (interactable == null)
                    {
                        interactable = matureCrop.AddComponent<InteractableObject>();
                    }
                    interactable.item = matureSeedData.CropToYield;

                    // Add and initialize tracker
                    MatureCropTracker tracker = matureCrop.GetComponent<MatureCropTracker>();
                    if (tracker == null)
                    {
                        tracker = matureCrop.AddComponent<MatureCropTracker>();
                    }

                    // Start instrument for this mature crop
                    if (AudioManager.Instance != null && matureSeedData.specificInstrument != null)
                    {
                        AudioSource source = AudioManager.Instance.RegisterCropInstrument(
                            matureSeedData.specificInstrument,
                            matureSeedData.cropSeason,
                            matureCrop.transform.position
                        );
                        tracker.Initialize(cropSave.landID, cropSave.seedToGrowName, source);

                        // Initialize controller
                        CropInstrumentController controller = matureCrop.GetComponent<CropInstrumentController>();
                        if (controller != null)
                        {
                            controller.Initialize(source, matureSeedData.cropSeason);
                        }

                        AudioManager.Instance.OnCropMature(matureSeedData.cropSeason);
                    }
                    else
                    {
                        tracker.Initialize(cropSave.landID, cropSave.seedToGrowName, null);
                    }
                }
                continue;
            }

            // Normal crop loading (not mature)
            if (cropSave.landID < 0 || cropSave.landID >= landPlots.Count)
            {
                Debug.LogWarning($"Skipping crop restoration for landID {cropSave.landID}: no matching land plot found.");
                continue;
            }

            Land landToPlant = landPlots[cropSave.landID];
            CropBehaviour crop = landToPlant.SpawnCrop();
            SeedData seedToGrow = (SeedData)InventoryManager.Instance.itemIndex.GetItemFromString(cropSave.seedToGrowName);
            crop.LoadCrop(cropSave.landID, seedToGrow, cropSave.currentStageIndex, cropSave.growth, cropSave.health, cropSave.isWilted, cropSave.hasMatureUnparented);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/