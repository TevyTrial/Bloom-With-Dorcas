using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] 
    private GameTimeStamp currentTime;
    public float timeScale = 1.0f;

    [Header("Sun Settings")]
    public Transform sunTransform; // Sun's Transform to rotate

    [Header("Skybox Settings")]
    public Material[] skyboxMaterials; // Array of skybox materials for different seasons

    [Tooltip("Time thresholds in minutes (0-1440) for each skybox transition")]
    public int[] timeThresholds = { 0, 360, 420, 1080, 1200, 1440 }; 
    // midnight, 6am, 7am, 6pm, 8pm, midnight

    [Range(0f, 1f)]
    public float skyboxBlendSpeed = 0.5f;

    //list of UI listeners to update when time changes
    List<ITimeTracker> listeners = new List<ITimeTracker>();

    //Season tracking
    private GameTimeStamp.Season previousSeason;

    private void Awake()
    {
        //If there is more than one instance, destroy the extra
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            //Set the static instance to this instance
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = new GameTimeStamp(1, GameTimeStamp.Season.Spring, 1, 6, 30);
        StartCoroutine(StartClock());
    }

    IEnumerator StartClock()
    {
        while(true)
        {
            yield return new WaitForSeconds(1/timeScale); //1 real second = 1 in-game minute, wait to tick
            Tick();
        }
    }

    // update clock by one minute
    public void Tick()
    {
        currentTime.UpdateClock();

        int timeInMinutes = GameTimeStamp.ConvertHoursToMinutes(currentTime.hour) + currentTime.minute;


        //Notify all listeners of time change
        foreach(var listener in listeners)
        {
            listener.ClockUpdate(currentTime);
        }

        //Check for season change and notify AudioManager
        if(currentTime.season != previousSeason) 
        {
            // Notify AudioManager of season change
            AudioManager.Instance.SetCurrentSeason(currentTime.season);
            previousSeason = currentTime.season;
        }
        
        //Update sun position based on time of day
        UpdateSun(timeInMinutes);

        //Update skybox based on time of day
        UpdateSkybox(timeInMinutes);

    }

    //Update sun based on time of day
    private void UpdateSun(int timeInMinutes)
    {
        if(sunTransform == null)
            return;
        float sunAngle = .25f * timeInMinutes - 90f; 
        // 0 min = -90 degrees, 720 min = 90 degrees, 1440 min = 270 degrees
        sunTransform.eulerAngles = new Vector3(sunAngle, 0, 0); // slight angle for more realistic lighting
    }
    
    //Update the skybox material based on time of day
    private void UpdateSkybox(int timeInMinutes)
    {
        if(skyboxMaterials == null || skyboxMaterials.Length == 0 || timeThresholds.Length < 2)
            return;

        //Find the current skybox index based on time thresholds
        int currentIndex = 0;

        for (int i = 0; i < timeThresholds.Length - 1; i++)
        {
            if(timeInMinutes >= timeThresholds[i] && timeInMinutes < timeThresholds[i + 1])
            {
                currentIndex = i;
                break;
            }
        }
        // Ensure we don't go out of bounds
        if (currentIndex >= skyboxMaterials.Length) currentIndex = skyboxMaterials.Length - 1;

        // Calculate blend factor for smooth transitions
        if (currentIndex < timeThresholds.Length - 1 && currentIndex < skyboxMaterials.Length - 1)
        {
            float timeRange = timeThresholds[currentIndex + 1] - timeThresholds[currentIndex];
            float timeProgress = (timeInMinutes - timeThresholds[currentIndex]) / timeRange;
            
            // Apply blend speed
            timeProgress = Mathf.Lerp(0f, 1f, timeProgress * skyboxBlendSpeed);

            // For now, we'll use immediate switching. Unity doesn't support skybox blending by default
            // You could implement custom skybox blending using a shader or rendering techniques
            RenderSettings.skybox = skyboxMaterials[currentIndex];
        }
        else
        {
            RenderSettings.skybox = skyboxMaterials[currentIndex];
        }
    }

    public GameTimeStamp GetGameTimeStamp()
    {
        return new GameTimeStamp(currentTime);
    }

    //Handle listener registration

    //Add the object to the list of listeners
    public void RegisterListener(ITimeTracker listener)
    {
        listeners.Add(listener);
    }

    //Remove the object from the list of listeners
    public void UnregisterListener(ITimeTracker listener)
    {
        listeners.Remove(listener);
    }
}

