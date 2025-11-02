using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandManager : MonoBehaviour
{
    public static LandManager Instance { get; private set; } 

    public static Tuple<List<LandSaveState>, List<CropSaveState>> farmData = null;

    [SerializeField] private List<Land> landPlots = new List<Land>();   
    
    // Save data for crops and lands
    [SerializeField] private List<LandSaveState> landData = new List<LandSaveState>();
    [SerializeField] private List<CropSaveState> cropData = new List<CropSaveState>();

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

        if (farmData != null)
        {
            // Load existing farm data
            ImportFarmData(farmData.Item1);
            ImportCropData(farmData.Item2);
        }
    }

    private void OnDestroy()
    {
        // Save unparented mature crops 
        SaveMatureCrops();
        // Store the current farm data before destruction
        farmData = new Tuple<List<LandSaveState>, List<CropSaveState>>(landData, cropData);
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
        foreach(Transform landTransform in transform)
        {
            Land land = landTransform.GetComponent<Land>();
            landPlots.Add(land);

            // Create a new LandSaveState and add it to the landData list
            landData.Add(new LandSaveState());

            // Assign an ID based on index
            land.id = landPlots.Count - 1;
        }
    }

    // Registers the crop onto the Instance
    public void RegisterCrop(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented)
    {
        cropData.Add(new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, Vector3.zero, Quaternion.identity));
    }

    public void DeregisterCrop(int landID)
    {
        cropData.RemoveAll(x => x.landID == landID);
    }

    // Update the land data
    public void OnLandStateChanged(int id, Land.LandState landStatus, GameTimeStamp lastWatered)
    {
        landData[id] = new LandSaveState(landStatus, lastWatered);
    }

    // Update the crop data
    public void OnCropStateChanged(int landID, string seedToGrowName, int currentStageIndex, int growth, int health, bool isWilted, bool hasMatureUnparented)
    {
        // Find its index in the list
        int cropIndex = cropData.FindIndex(x => x.landID == landID);
        
        if (cropIndex != -1)
        {
            // Update existing entry, preserve position/rotation if it exists
            CropSaveState existing = cropData[cropIndex];
            cropData[cropIndex] = new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, existing.maturePosition, existing.matureRotation);
        }
        else
        {
            // If not found, add new
            cropData.Add(new CropSaveState(landID, seedToGrowName, currentStageIndex, growth, health, isWilted, hasMatureUnparented, Vector3.zero, Quaternion.identity));
        }
    }

    //Load over the static farmData onto the instance's landdata
    public void ImportFarmData(List<LandSaveState> landDatasetToLoad)
    {
        for(int i = 0; i < landDatasetToLoad.Count; i++)
        {
            //Get the individual land save state
            LandSaveState landStateToLoad = landDatasetToLoad[i];
            //Load the land data into the corresponding land plot
            landPlots[i].LoadLandData(landStateToLoad.landStatus, landStateToLoad.lastWatered);
        }
        landData = landDatasetToLoad;
    }

    // Load crop data onto the instance's cropdata
    public void ImportCropData(List<CropSaveState> cropDatasetToLoad)
    {
        cropData = cropDatasetToLoad;
        foreach(CropSaveState cropSave in cropDatasetToLoad)
        {
            // Handle mature unparented crops
            if (cropSave.hasMatureUnparented)
            {
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