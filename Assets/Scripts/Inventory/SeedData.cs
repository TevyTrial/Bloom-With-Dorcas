using UnityEngine;

[CreateAssetMenu(menuName = "Items/Seed")]
public class SeedData : ItemData
{   
    //the plant that this seed will grow into
    public ItemData CropToYield;

    //how many days it takes to grow
    public int growTimeInDays;

    [Header("Growth Stages")]
    [Tooltip("Define all visual stages for this crop (in order). First stage is always the seed.")]
    public GameObject[] growthStageModels; 
    public GameObject WiltedModel;

    [Header("Instrument Assignment")]
    [Tooltip("Leave null to assign random instrument from current season. Or specify a particular instrument track.")]
    public InstrumentTrack specificInstrument;
    [Header("Crop Season")]
    public GameTimeStamp.Season cropSeason;
}