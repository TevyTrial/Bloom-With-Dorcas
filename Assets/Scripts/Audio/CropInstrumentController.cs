using UnityEngine;

public class CropInstrumentController : MonoBehaviour
{
    private AudioSource instrumentSource;

    public void Initialize(AudioSource source)
    {
        instrumentSource = source;
    }

    private void OnDestroy()
    {
        // Stop the instrument when the mature crop is harvested/destroyed
        if (AudioManager.Instance != null && instrumentSource != null)
        {
            AudioManager.Instance.UnregisterCropInstrument(instrumentSource);
            instrumentSource = null;
            Debug.Log("Stopped instrument on crop harvest");
        }
    }
}