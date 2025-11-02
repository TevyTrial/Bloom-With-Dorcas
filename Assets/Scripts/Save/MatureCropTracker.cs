using UnityEngine;

public class MatureCropTracker : MonoBehaviour
{
    public int landID;
    public string seedName;
    private AudioSource instrumentSource;

    public void Initialize(int landID, string seedName, AudioSource source)
    {
        this.landID = landID;
        this.seedName = seedName;
        this.instrumentSource = source;
    }

    private void OnDestroy()
    {
        // When the mature crop is picked up/destroyed, deregister it
        if (LandManager.Instance != null)
        {
            LandManager.Instance.DeregisterCrop(landID);
        }

        // Stop the instrument
        if (AudioManager.Instance != null && instrumentSource != null)
        {
            AudioManager.Instance.UnregisterCropInstrument(instrumentSource);
        }
    }
}