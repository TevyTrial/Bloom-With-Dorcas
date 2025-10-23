using UnityEngine;

public class CropInstrumentController : MonoBehaviour
{
    private AudioSource source;
    private GameTimeStamp.Season cropSeason;

    public void Initialize(AudioSource source, GameTimeStamp.Season cropSeason)
    {
        this.cropSeason = cropSeason;
        this.source = source;
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