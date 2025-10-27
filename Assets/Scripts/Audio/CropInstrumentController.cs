using UnityEngine;

public class CropInstrumentController : MonoBehaviour
{
    private AudioSource source;
    private GameTimeStamp.Season cropSeason;

    [SerializeField] private GameObject particleSystemObject;

    public void Initialize(AudioSource source, GameTimeStamp.Season cropSeason)
    {
        this.cropSeason = cropSeason;
        this.source = source;

        // Check if particle system is assigned
        if (particleSystemObject != null)
        {
            // Get current season
            GameTimeStamp.Season currentSeason = TimeManager.Instance.GetGameTimeStamp().season;
            particleSystemObject.SetActive(currentSeason == cropSeason);
        }
    }
  
    private void OnDestroy()
    {
        // Stop the instrument when the mature crop is harvested/destroyed
        if (AudioManager.Instance != null)
        {
            if (source != null)
            {
                AudioManager.Instance.UnregisterCropInstrument(source);
            }
            AudioManager.Instance.OnCropHarvested(cropSeason);
        }
    }
}