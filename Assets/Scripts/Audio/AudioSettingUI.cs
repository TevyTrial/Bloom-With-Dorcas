using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;

    // Mixer parameter names (must match your AudioMixer exposed parameters)
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string AMBIENT_VOLUME = "AmbientVolume";

    private void Start()
    {
        if (audioMixer == null)
        {
            Debug.LogError("[AudioSettingUI] AudioMixer not assigned!");
            return;
        }

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
        // Load saved values from PlayerPrefs (default 1.0 = 100%)
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

        // Apply the loaded volumes to AudioMixer
        SetMixerVolume(MASTER_VOLUME, masterVolume);
        SetMixerVolume(MUSIC_VOLUME, musicVolume);
        SetMixerVolume(SFX_VOLUME, sfxVolume);
        SetMixerVolume(AMBIENT_VOLUME, ambientVolume);
    }

    private void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume(MASTER_VOLUME, value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        SetMixerVolume(MUSIC_VOLUME, value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume(SFX_VOLUME, value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        // Play a test SFX sound when adjusting (only if volume > 0)
        if (value > 0.01f && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlowingSFX();
        }
    }

    private void OnAmbientVolumeChanged(float value)
    {
        SetMixerVolume(AMBIENT_VOLUME, value);
        PlayerPrefs.SetFloat("AmbientVolume", value);
    }

    /// <summary>
    /// Convert linear slider value (0-1) to decibel scale and set mixer parameter
    /// </summary>
    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (audioMixer == null) return;

        // Convert linear (0-1) to decibels (-80dB to 0dB)
        // Using logarithmic scale for natural volume perception
        float decibels = sliderValue > 0.0001f 
            ? Mathf.Log10(sliderValue) * 20f 
            : -80f; // Mute at 0

        audioMixer.SetFloat(parameterName, decibels);
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