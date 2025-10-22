using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Seasonal Music")]
    [SerializeField] private AudioClip springMusic;
    [SerializeField] private AudioClip summerMusic;
    [SerializeField] private AudioClip fallMusic;
    [SerializeField] private AudioClip winterMusic;

    [Header("Seasonal Instrument System")]
    [SerializeField] private SeasonalSongData springSong;
    [SerializeField] private SeasonalSongData summerSong;
    [SerializeField] private SeasonalSongData fallSong;
    [SerializeField] private SeasonalSongData winterSong;

    // Synchronized playback tracking
    private SeasonalSongData currentSong;
    private List<AudioSource> activeInstrumentSources = new List<AudioSource>();
    private float songStartTime;
    private bool isSongPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize the song system with Spring as default
            if (springSong != null)
            {
                currentSong = springSong;
                songStartTime = Time.time;
                isSongPlaying = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Keep all instrument tracks synchronized
        if (isSongPlaying && currentSong != null)
        {
            float currentPlaybackTime = Time.time - songStartTime;
            float songLength = currentSong.songLengthInSeconds;

            // Loop the song
            if (currentPlaybackTime >= songLength)
            {
                songStartTime = Time.time;
                SyncAllInstruments();
            }
        }
    }

    // Change background music based on season
    public void PlaySeasonalMusic(Season season)
    {
        if (musicSource == null) return;

        AudioClip clipToPlay = season switch
        {
            Season.Spring => springMusic,
            Season.Summer => summerMusic,
            Season.Fall => fallMusic,
            Season.Winter => winterMusic,
            _ => springMusic
        };

        if (clipToPlay != null && musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
        }

        // Also switch the instrument song data
        SwitchSeasonalSong(season);
    }

    // Switch to a new seasonal song and restart synchronization
    private void SwitchSeasonalSong(Season season)
    {
        currentSong = season switch
        {
            Season.Spring => springSong,
            Season.Summer => summerSong,
            Season.Fall => fallSong,
            Season.Winter => winterSong,
            _ => springSong
        };

        // Restart song timeline
        songStartTime = Time.time;
        isSongPlaying = true;

        // Resync all existing instruments
        SyncAllInstruments();
    }

    // Register a crop's instrument track and sync it to the current song
    public AudioSource RegisterCropInstrument(InstrumentTrack track)
    {
        if (track == null || track.audioClip == null)
        {
            Debug.LogWarning("Cannot register instrument: track or audioClip is null");
            return null;
        }

        // Initialize song system if not started
        if (currentSong == null)
        {
            if (springSong != null)
            {
                currentSong = springSong;
                songStartTime = Time.time;
                isSongPlaying = true;
            }
            else
            {
                Debug.LogError("No seasonal song data assigned to AudioManager!");
                return null;
            }
        }

        // Create a new AudioSource for this instrument
        GameObject instrumentObj = new GameObject($"Instrument_{track.audioClip.name}");
        instrumentObj.transform.SetParent(transform);
        AudioSource source = instrumentObj.AddComponent<AudioSource>();

        // Configure the AudioSource
        source.clip = track.audioClip;
        source.loop = true;
        source.volume = track.volume;
        source.spatialBlend = 0f; // 2D sound
        source.playOnAwake = false;

        // Sync to current playback time
        float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;
        source.time = currentPlaybackTime;
        source.Play();

        activeInstrumentSources.Add(source);
        
        return source;
    }

    // Unregister a crop's instrument when harvested/destroyed
    public void UnregisterCropInstrument(AudioSource source)
    {
        if (source != null && activeInstrumentSources.Contains(source))
        {
            activeInstrumentSources.Remove(source);
            Destroy(source.gameObject);
        }
    }

    // Sync all active instruments to the current timeline
    private void SyncAllInstruments()
    {
        if (currentSong == null) return;

        float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;

        foreach (var source in activeInstrumentSources)
        {
            if (source != null && source.clip != null)
            {
                source.time = currentPlaybackTime;
                if (!source.isPlaying)
                {
                    source.Play();
                }
            }
        }
    }

    // Get a random instrument track from the current season's song
    public InstrumentTrack GetRandomInstrumentTrack()
    {
        if (currentSong == null || currentSong.instrumentTracks.Length == 0)
            return null;

        int randomIndex = Random.Range(0, currentSong.instrumentTracks.Length);
        return currentSong.instrumentTracks[randomIndex];
    }

    // Volume controls
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetInstrumentVolume(float volume)
    {
        foreach (var source in activeInstrumentSources)
        {
            if (source != null)
            {
                source.volume = Mathf.Clamp01(volume);
            }
        }
    }
}

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}