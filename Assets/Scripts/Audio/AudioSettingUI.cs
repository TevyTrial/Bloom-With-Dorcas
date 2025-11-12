using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;

    private void Start()
    {
        // Initialize sliders with current values
        LoadCurrentVolumes();

        // Add listeners to sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (ambientVolumeSlider != null)
            ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
    }

    private void LoadCurrentVolumes()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AudioSettingsUI] AudioManager instance not found");
            return;
        }

        // Load saved values from PlayerPrefs with default fallback of 1.0 (100%)
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f);

        // Set slider values without triggering callbacks
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        
        if (ambientVolumeSlider != null)
            ambientVolumeSlider.SetValueWithoutNotify(ambientVolume);

        // Apply the loaded volumes to AudioManager
        AudioManager.Instance.SetMasterVolume(masterVolume);
        AudioManager.Instance.SetMusicVolume(musicVolume);
        AudioManager.Instance.SetSFXVolume(sfxVolume);
        AudioManager.Instance.SetAmbientVolume(ambientVolume);
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            PlayerPrefs.SetFloat("SFXVolume", value);
            
            // Play a test SFX sound when adjusting
            AudioManager.Instance.PlayPlowingSFX();
        }
    }

    private void OnAmbientVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAmbientVolume(value);
            PlayerPrefs.SetFloat("AmbientVolume", value);
        }
    }

    private void OnDestroy()
    {
        // Save settings when UI is destroyed
        PlayerPrefs.Save();

        // Remove listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        
        if (ambientVolumeSlider != null)
            ambientVolumeSlider.onValueChanged.RemoveListener(OnAmbientVolumeChanged);
    }

    // Reset to default volumes
    public void ResetToDefaults()
    {
        SetAllVolumes(1f, 1f, 1f, 1f);
    }

    private void SetAllVolumes(float master, float music, float sfx, float ambient)
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (musicVolumeSlider != null) musicVolumeSlider.value = music;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        if (ambientVolumeSlider != null) ambientVolumeSlider.value = ambient;
        
        PlayerPrefs.Save();
    }
}