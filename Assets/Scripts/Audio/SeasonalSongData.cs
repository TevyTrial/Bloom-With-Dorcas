using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Seasonal Song Data")]
public class SeasonalSongData : ScriptableObject
{
    [Header("Song Information")]
    public string songName;
    public float songLengthInSeconds = 60f;
    
    [Header("Instrument Tracks")]
    [Tooltip("All instrument tracks that make up this song. Each crop will play one track.")]
    public InstrumentTrack[] instrumentTracks;
}

[System.Serializable]
public class InstrumentTrack
{
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 0.7f;
}