using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private bool persistAcrossScenes = false; 
    [SerializeField] private string[] activeInScenes = new string[] { "Garden" }; // NEW: Scenes where this manager is active

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    private AudioSource footstepSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip walkingSFX;
    [SerializeField] private AudioClip wateringSFX;
    [SerializeField] private AudioClip plowingSFX;

    [Header("Seasonal Instrument System")]
    [SerializeField] private SeasonalSongData springSong;
    [SerializeField] private SeasonalSongData summerSong;
    [SerializeField] private SeasonalSongData fallSong;
    [SerializeField] private SeasonalSongData winterSong;

    [Header("Instrument Volume Settings")]
    [SerializeField] private float maxTotalInstrumentVolume = 1.0f;
    [SerializeField] private int cropCountForMaxVolume = 10;
    [SerializeField] private float minDistanceFor3D = 1f;
    [SerializeField] private float maxDistanceFor3D = 30f;

    [Header("Performance Settings")]
    [SerializeField] private int audioSourcePoolSize = 50; // Pre-create pool
    [SerializeField] private float volumeUpdateDelay = 0.1f; // Batch volume updates

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

    // OPTIMIZATION: Object pooling for AudioSources
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private GameObject poolContainer;

    // OPTIMIZATION: Batch volume updates
    private bool volumeUpdatePending = false;
    private float lastVolumeUpdateTime = 0f;

    // OPTIMIZATION: Cache current season count
    private int currentSeasonCropCount = 0;

    private GameTimeStamp.Season currentGameSeason = GameTimeStamp.Season.Spring;

    private void Awake()
    {
        // Check if this AudioManager should be active in current scene
        string currentScene = SceneManager.GetActiveScene().name;
        bool shouldBeActive = System.Array.Exists(activeInScenes, scene => scene == currentScene);

        if (!shouldBeActive)
        {
            Debug.Log($"[AudioManager] Not active in scene '{currentScene}' - destroying");
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            
            // Only persist if enabled
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

             // Create dedicated footstep AudioSource
            GameObject footstepObj = new GameObject("FootstepSource");
            footstepObj.transform.SetParent(transform);
            footstepSource = footstepObj.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f; // 2D audio
            footstepSource.loop = false;
            
            // OPTIMIZATION: Initialize object pool
            InitializeAudioSourcePool();
            
            // OPTIMIZATION: Preload all audio clips
            PreloadAudioClips();
            
            if (ambientSource != null && ambientSource.clip != null)
            {
                ambientSource.Play();
            }
            
            currentSong = springSong;
            currentGameSeason = GameTimeStamp.Season.Spring;
            currentSeasonCropCount = matureCropCounts[currentGameSeason];

            Debug.Log($"[AudioManager] Initialized in scene '{currentScene}'");
        }
        else
        {
            // If another instance exists and we're scene-specific, destroy this one
            if (!persistAcrossScenes)
            {
                Debug.Log($"[AudioManager] Another instance exists, destroying scene-specific manager");
            }
            Destroy(gameObject);
        }
    }

    // OPTIMIZATION: Pre-create AudioSource pool
    private void InitializeAudioSourcePool()
    {
        poolContainer = new GameObject("AudioSourcePool");
        poolContainer.transform.SetParent(transform);
        
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource source = CreatePooledAudioSource();
            source.gameObject.SetActive(false);
            audioSourcePool.Enqueue(source);
        }
        
        Debug.Log($"AudioManager: Initialized pool with {audioSourcePoolSize} AudioSources");
    }

    private AudioSource CreatePooledAudioSource()
    {
        GameObject obj = new GameObject("PooledAudioSource");
        obj.transform.SetParent(poolContainer.transform);
        
        AudioSource source = obj.AddComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 1f;
        source.playOnAwake = false;
        source.minDistance = minDistanceFor3D;
        source.maxDistance = maxDistanceFor3D;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.dopplerLevel = 0f;
        
        return source;
    }

    // OPTIMIZATION: Preload all audio clips to prevent loading stutter
    private void PreloadAudioClips()
    {
        PreloadSongData(springSong);
        PreloadSongData(summerSong);
        PreloadSongData(fallSong);
        PreloadSongData(winterSong);
        
        Debug.Log("AudioManager: All audio clips preloaded");
    }

    private void PreloadSongData(SeasonalSongData songData)
    {
        if (songData == null) return;
        
        // Access all instrument tracks to force Unity to load them
        foreach (var track in songData.instrumentTracks)
        {
            if (track != null && track.audioClip != null)
            {
                track.audioClip.LoadAudioData();
            }
        }
    }

    private void Update()
    {
        if (isSongPlaying && currentSong != null)
        {
            float currentPlaybackTime = Time.time - songStartTime;
            float songLength = currentSong.songLengthInSeconds;

            if (currentPlaybackTime >= songLength)
            {
                songStartTime = Time.time;
                SyncAllInstruments();
            }
        }

        // OPTIMIZATION: Batch volume updates instead of updating immediately
        if (volumeUpdatePending && Time.time - lastVolumeUpdateTime >= volumeUpdateDelay)
        {
            PerformVolumeUpdate();
            volumeUpdatePending = false;
        }
    }

    public void OnCropMature(GameTimeStamp.Season cropSeason)
    {
        matureCropCounts[cropSeason]++;
        
        // OPTIMIZATION: Cache current season count
        if (cropSeason == currentGameSeason)
        {
            currentSeasonCropCount++;
        }
        
        if (matureCropCounts[cropSeason] == 1)
        {
            CheckAndPlaySeasonalMusic();
        }

        // OPTIMIZATION: Schedule batched volume update
        ScheduleVolumeUpdate();
    }

    public void OnCropHarvested(GameTimeStamp.Season cropSeason)
    {
        if (matureCropCounts[cropSeason] > 0)
        {
            matureCropCounts[cropSeason]--;
            
            // OPTIMIZATION: Cache current season count
            if (cropSeason == currentGameSeason)
            {
                currentSeasonCropCount--;
            }
            
            if (matureCropCounts[cropSeason] == 0)
            {
                CheckAndPlaySeasonalMusic();
            }
            
            ScheduleVolumeUpdate();
        }
    }

    // OPTIMIZATION: Batched volume updates
    private void ScheduleVolumeUpdate()
    {
        volumeUpdatePending = true;
        lastVolumeUpdateTime = Time.time;
    }

    private void PerformVolumeUpdate()
    {
        // Cache count to avoid repeated dictionary lookups
        int cropCount = currentSeasonCropCount;
        if (cropCount == 0) return;

        float volumeScaling = Mathf.Min(1f, (float)cropCountForMaxVolume / cropCount);

        // Update all volumes in one pass
        for (int i = 0; i < activeInstrumentSources.Count; i++)
        {
            AudioSource source = activeInstrumentSources[i];
            if (source != null && source.clip != null)
            {
                float baseVolume = originalVolumes[source];
                float normalizedVolume = (baseVolume * volumeScaling * maxTotalInstrumentVolume);
                source.volume = Mathf.Clamp(normalizedVolume, 0f, baseVolume);
            }
        }
    }

    private void CheckAndPlaySeasonalMusic()
    {
        bool hasSeasonalCrops = currentSeasonCropCount > 0;
        
        if (hasSeasonalCrops && !isSongPlaying)
        {
            songStartTime = Time.time;
            isSongPlaying = true;
            SyncAllInstruments();
        }
        else if (!hasSeasonalCrops && isSongPlaying)
        {
            isSongPlaying = false;
            StopAllInstruments();
        }
    }
    
    // OPTIMIZATION: Use cached count
    private float GetNormalizedVolume(float baseVolume)
    {
        int totalCrops = currentSeasonCropCount;
        if (totalCrops == 0) return baseVolume;

        float volumeScaling = Mathf.Min(1f, (float)cropCountForMaxVolume / totalCrops);
        float normalizedVolume = (baseVolume * volumeScaling * maxTotalInstrumentVolume);

        return Mathf.Clamp(normalizedVolume, 0f, baseVolume);
    }

    // OPTIMIZATION: Use pooled AudioSources instead of instantiating
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

        if (cropSeason != currentGameSeason)
        {
            return null;
        }

        // OPTIMIZATION: Get from pool instead of instantiating
        AudioSource source = GetPooledAudioSource();
        if (source == null)
        {
            Debug.LogWarning("AudioSource pool exhausted, creating new source");
            source = CreatePooledAudioSource();
        }

        // Configure the AudioSource
        source.gameObject.SetActive(true);
        source.transform.position = cropPosition;
        source.clip = track.audioClip;
        source.volume = track.volume;

        originalVolumes[source] = track.volume;
        source.volume = GetNormalizedVolume(track.volume);

        activeInstrumentSources.Add(source);

        // OPTIMIZATION: Schedule batched update instead of immediate
        ScheduleVolumeUpdate();

        if (isSongPlaying)
        {
            float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;
            source.time = currentPlaybackTime;
            source.Play();
        }
        else if (currentSeasonCropCount >= 1)
        {
            songStartTime = Time.time;
            isSongPlaying = true;
            SyncAllInstruments();
        }
        
        return source;
    }

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }
        return null;
    }

    // OPTIMIZATION: Return to pool instead of destroying
    public void UnregisterCropInstrument(AudioSource source)
    {
        if (source != null && activeInstrumentSources.Contains(source))
        {
            activeInstrumentSources.Remove(source);
            originalVolumes.Remove(source);
            
            // Return to pool
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            audioSourcePool.Enqueue(source);
        }
        
        ScheduleVolumeUpdate();
    }

    private void SyncAllInstruments()
    {
        if (currentSong == null || !isSongPlaying) return;

        float currentPlaybackTime = (Time.time - songStartTime) % currentSong.songLengthInSeconds;

        // OPTIMIZATION: Use for loop instead of foreach (less GC)
        for (int i = 0; i < activeInstrumentSources.Count; i++)
        {
            AudioSource source = activeInstrumentSources[i];
            if (source != null && source.clip != null)
            {
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

    private void StopAllInstruments()
    {
        for (int i = 0; i < activeInstrumentSources.Count; i++)
        {
            if (activeInstrumentSources[i] != null)
            {
                activeInstrumentSources[i].Stop();
            }
        }
    }

    public void SetCurrentSeason(GameTimeStamp.Season season)
    {
        currentGameSeason = season;
        currentSeasonCropCount = matureCropCounts[currentGameSeason];

        currentSong = season switch
        {
            GameTimeStamp.Season.Spring => springSong,
            GameTimeStamp.Season.Summer => summerSong,
            GameTimeStamp.Season.Fall => fallSong,
            GameTimeStamp.Season.Winter => winterSong,
            _ => springSong
        };

        StopAllInstruments();

        // OPTIMIZATION: Return all to pool instead of destroying
        foreach (var source in activeInstrumentSources)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
                audioSourcePool.Enqueue(source);
            }
        }
        
        isSongPlaying = false;
        activeInstrumentSources.Clear();
        originalVolumes.Clear();

        CheckAndPlaySeasonalMusic();
    }
    #region SFX Volume Controls
       public void PlaySFX(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] PlaySFX called with null clip");
                return;
            }

            // If sfxSource wasn't assigned in inspector, create a simple 2D source as fallback
            if (sfxSource == null)
            {
                GameObject go = new GameObject("SFXSource_Temp");
                go.transform.SetParent(transform);
                sfxSource = go.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D SFX
            }

            if (!sfxSource.gameObject.activeInHierarchy)
                sfxSource.gameObject.SetActive(true);

            if (AudioListener.pause)
                Debug.LogWarning("[AudioManager] AudioListener.pause is true; SFX may not be audible");

            Debug.Log($"[AudioManager] PlaySFX: {clip.name} volume={volume}");
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        //Specific SFX methods
        public void PlayWalkingSFX()
        {
            // Use dedicated footstep source instead of shared sfxSource
            if (footstepSource != null && walkingSFX != null)
            {
                if (!footstepSource.isPlaying)
                {
                    footstepSource.PlayOneShot(walkingSFX, 1.0f);
                }
            }
        }
        public void PlayWateringSFX()
        {
            PlaySFX(wateringSFX, 1f);
        }
        public void PlayPlowingSFX()
        {
            PlaySFX(plowingSFX, 0.6f);
        }

        public void StopWalkingSFX()
        {
            if (footstepSource != null && footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }

    #endregion

    #region Volume Controls
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Convert 0-1 linear to decibels: -80dB (silent) to 0dB (full)
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MusicVolume", dB);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }
    }

    public void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("AmbientVolume", dB);
        }
    }
    
    public float GetMasterVolume()
    {
        if (audioMixer != null && audioMixer.GetFloat("MasterVolume", out float dB))
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        return 1f;
    }

    public float GetMusicVolume()
    {
        if (audioMixer != null && audioMixer.GetFloat("MusicVolume", out float dB))
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        return 1f;
    }

    public float GetSFXVolume()
    {
        if (audioMixer != null && audioMixer.GetFloat("SFXVolume", out float dB))
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        return 1f;
    }

    public float GetAmbientVolume()
    {
        if (audioMixer != null && audioMixer.GetFloat("AmbientVolume", out float dB))
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        return 1f;
    }

    #endregion
}