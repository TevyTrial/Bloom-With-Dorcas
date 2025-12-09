using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Reflection;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;

    // Mixer parameter names
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string AMBIENT_VOLUME = "AmbientVolume";

    private void Start()
    {
        if (audioMixer == null)
        {
            return;
        }
        
        LoadSavedVolumes();
    }

    private void LoadSavedVolumes()
    {
        if (masterVolumeSlider != null)
        {
            float savedValue = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.value = savedValue;
            OnMasterVolumeChanged(savedValue);
        }

        if (musicVolumeSlider != null)
        {
            float savedValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.value = savedValue;
            OnMusicVolumeChanged(savedValue);
        }

        if (sfxVolumeSlider != null)
        {
            float savedValue = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.value = savedValue;
            OnSFXVolumeChanged(savedValue);
        }

        if (ambientVolumeSlider != null)
        {
            float savedValue = PlayerPrefs.GetFloat("AmbientVolume", 1f);
            ambientVolumeSlider.value = savedValue;
            OnAmbientVolumeChanged(savedValue);
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume(MASTER_VOLUME, value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        SetMixerVolume(MUSIC_VOLUME, value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume(SFX_VOLUME, value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        if (value > 0.01f && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlowingSFX();
        }
    }

    public void OnAmbientVolumeChanged(float value)
    {
        SetMixerVolume(AMBIENT_VOLUME, value);
        PlayerPrefs.SetFloat("AmbientVolume", value);
    }

    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (audioMixer == null) return;

        float decibels = LinearToDecibel(sliderValue);
        audioMixer.SetFloat(parameterName, decibels);
    }

    private float LinearToDecibel(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }

    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 1f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
        if (ambientVolumeSlider != null) ambientVolumeSlider.value = 1f;
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}