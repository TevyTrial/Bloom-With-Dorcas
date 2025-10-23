using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;// Crops Seasonal Music
    [SerializeField] private AudioSource sfxSource;// General SFX
    [SerializeField] private AudioSource ambientSource;// Ambient Sounds

    [Header("Seasonal Instrument System")]
    [SerializeField] private SeasonalSongData springSong;
    [SerializeField] private SeasonalSongData summerSong;
    [SerializeField] private SeasonalSongData fallSong;
    [SerializeField] private SeasonalSongData winterSong;

    [Header("Instrument Volume Settings")]
    [SerializeField] private float maxTotalInstrumentVolume = 1.0f;// Max combined volume of all instruments
    [SerializeField] private int cropCountForMaxVolume = 10; // Number of mature crops to reach max volume
    [SerializeField] private float minDistanceFor3D = 1f; // Min distance for 3D sound attenuation
    [SerializeField] private float maxDistanceFor3D = 30f; // Max distance for 3D sound attenuation

    // Synchronized playback tracking
    private SeasonalSongData currentSong;
    private List<AudioSource> activeInstrumentSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();
    private float songStartTime;
    private bool isSongPlaying = false;
    
    // Track mature crops by season
    private Dictionary<GameTimeStamp.Season, int> matureCropCounts = new Dictionary<GameTimeStamp.Season, int>()
    {
        { GameTimeStamp.Season.Spring, 0 },
        { GameTimeStamp.Season.Summer, 0 },
        { GameTimeStamp.Season.Fall, 0 },
        { GameTimeStamp.Season.Winter, 0 }
    };

    //Current game season
    private GameTimeStamp.Season currentGameSeason = GameTimeStamp.Season.Spring;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Start ambient sound
            if (ambientSource != null && ambientSource.clip != null)
            {
                ambientSource.Play();
            }
            
            // Initialize with Spring season
            currentSong = springSong;
            currentGameSeason = GameTimeStamp.Season.Spring;
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

    // Called when a crop reaches mature state
    public void OnCropMature(GameTimeStamp.Season cropSeason)
    {
        matureCropCounts[cropSeason]++;
        
        // Start playing seasonal music if this is the first mature crop of this season
        if (matureCropCounts[cropSeason] == 1)
        {
            CheckAndPlaySeasonalMusic();
        }

        //Recalculate instrument volumes
        UpdateAllInstrumentVolumes();
    }

    // Called when a mature crop is harvested or destroyed
    public void OnCropHarvested(GameTimeStamp.Season cropSeason)
    {
        if (matureCropCounts[cropSeason] > 0)
        {
            matureCropCounts[cropSeason]--;
            
            // Stop seasonal music if no more mature crops of this season
            if (matureCropCounts[cropSeason] == 0)
            {
                CheckAndPlaySeasonalMusic();
            }
            //Recalculate instrument volumes
            UpdateAllInstrumentVolumes();
        }
    }

    // Check if we should play seasonal music based on mature crop counts
    private void CheckAndPlaySeasonalMusic()
    {
        //Check current season mature crops
        bool hasSeasonalCrops = matureCropCounts[currentGameSeason] > 0;
        if (hasSeasonalCrops && !isSongPlaying)
        {
            // Start seasonal music
            songStartTime = Time.time;
            isSongPlaying = true;
            SyncAllInstruments();
        }
        else if (!hasSeasonalCrops && isSongPlaying)
        {
            // Stop seasonal music
            isSongPlaying = false;
            StopAllInstruments();
        }
    }

    //Calcuate normalized volume based on number of mature crops
    private float GetNormalizedVolume(float baseVolume) {
        int totalCrops = activeInstrumentSources.Count;
        if (totalCrops == 0) return baseVolume;

       // Calculate volume scaling factor
        // As crops increase, individual volume decreases to maintain max total volume
        float volumeScaling = Mathf.Min(1f, (float)cropCountForMaxVolume / totalCrops);
        
        // Apply scaling but ensure we don't exceed max total volume
        float normalizedVolume = (baseVolume * volumeScaling * maxTotalInstrumentVolume);

        return Mathf.Clamp(normalizedVolume, 0f, baseVolume);
    }

    // Update volumes for all active instruments
    private void UpdateAllInstrumentVolumes()
    {
        foreach (var source in activeInstrumentSources)
        {
            if (source != null && source.clip != null)
            {
                //Get the base volume from the clip
                float baseVolume = source.volume;
                source.volume = GetNormalizedVolume(baseVolume);
            }
        }
    }

    // Register a crop's instrument track and sync it to the current song
    public AudioSource RegisterCropInstrument(InstrumentTrack track, GameTimeStamp.Season cropSeason, Vector3 cropPosition)
    {
        if (track == null || track.audioClip == null)
        {
            Debug.LogWarning("Cannot register instrument: track or audioClip is null");
            return null;
        }

        if (currentSong == null)
        {
            Debug.LogError("No seasonal song data assigned to AudioManager!");
            return null;
        } 

        //Only play instruments for crops of the current season
        if (cropSeason != currentGameSeason)
        {
            Debug.Log($"Crop season {cropSeason} doesn't match current season {currentGameSeason}. Not playing instrument.");
            return null;
        }

        // Create a new AudioSource for this instrument
        GameObject instrumentObj = new GameObject($"Instrument_{track.audioClip.name}");
        instrumentObj.transform.SetParent(transform);
        instrumentObj.transform.position = cropPosition;    

        AudioSource source = instrumentObj.AddComponent<AudioSource>();

        // Configure the AudioSource
        source.clip = track.audioClip;
        source.loop = true;
        source.volume = track.volume;
        source.spatialBlend = 1f; // 3D sound
        source.playOnAwake = false;

        // Set 3D sound settings
        source.minDistance = minDistanceFor3D;
        source.maxDistance = maxDistanceFor3D;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.dopplerLevel = 0f;

        // Set initial volume based on current number of mature crops
        originalVolumes[source] = track.volume;
        source.volume = GetNormalizedVolume(track.volume);

        // Add to active instruments list
        activeInstrumentSources.Add(source);

        // Update all instrument volumes
        UpdateAllInstrumentVolumes();

        // Only play if seasonal music is active
        if (isSongPlaying)
        {
            //Calculate current playback time to sync
            float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;
            //set the audio source time and play
            source.time = currentPlaybackTime;
            source.Play();
        }
        else if (matureCropCounts[currentGameSeason] >= 1)
        {
            // Start seasonal music if not already playing
            songStartTime = Time.time;
            isSongPlaying = true;
            // Sync all instruments including this new one
            SyncAllInstruments();
        }
        
        return source;
    }

    // Unregister a crop's instrument when harvested/destroyed
    public void UnregisterCropInstrument(AudioSource source)
    {
        if (source != null && activeInstrumentSources.Contains(source))
        {
            activeInstrumentSources.Remove(source);
            originalVolumes.Remove(source);
            Destroy(source.gameObject);
        }
        // Update all instrument volumes
        UpdateAllInstrumentVolumes();
    }

    // Sync all active instruments to the current timeline
    private void SyncAllInstruments()
    {
        if (currentSong == null || !isSongPlaying) return;

        float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;

        foreach (var source in activeInstrumentSources)
        {
            if (source != null && source.clip != null)
            {
                //only adjust time if there's a significant difference to avoid audio glitches
                float timeDifference = Mathf.Abs(source.time - currentPlaybackTime);
                if (timeDifference > 0.1f)
                {
                    source.time = currentPlaybackTime;
                }
                
                if (!source.isPlaying)
                {
                    source.Play();
                }
            }
        }
    }

    // Stop all instruments
    private void StopAllInstruments()
    {
        foreach (var source in activeInstrumentSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }

    // Called when the game season changes
    public void SetCurrentSeason(GameTimeStamp.Season season)
    {
        currentGameSeason = season;

        currentSong = season switch
        {
            GameTimeStamp.Season.Spring => springSong,
            GameTimeStamp.Season.Summer => summerSong,
            GameTimeStamp.Season.Fall => fallSong,
            GameTimeStamp.Season.Winter => winterSong,
            _ => springSong
        };

        // Stop all out-of-season instruments and restart music system
        StopAllInstruments();

        // Destroy all audio source game objects
        foreach (var source in activeInstrumentSources)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }
        isSongPlaying = false;
        activeInstrumentSources.Clear();
        originalVolumes.Clear();

        //Check if we should play music for the new season
        CheckAndPlaySeasonalMusic();
    }

    // Volume controls
    public void SetAmbientSoundVolume(float volume)
    {
        if (ambientSource != null)
            ambientSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetInstrumentVolume(float volume)
    {
        maxTotalInstrumentVolume = Mathf.Clamp01(volume);
        UpdateAllInstrumentVolumes();
    }
}
