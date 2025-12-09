using System.Collections.Generic;
using UnityEngine;

public class tree : MonoBehaviour
{
    [SerializeField] private GameObject springTree;
    [SerializeField] private GameObject fallTree;
    [SerializeField] private GameObject winterTree;

    void Update()
    {
        if (TimeManager.Instance == null) return;

        GameTimeStamp.Season currentSeason = TimeManager.Instance.GetGameTimeStamp().season;

        // Enable/disable tree models based on the current season
        springTree.SetActive(currentSeason == GameTimeStamp.Season.Spring || currentSeason == GameTimeStamp.Season.Summer);
        fallTree.SetActive(currentSeason == GameTimeStamp.Season.Fall);
        winterTree.SetActive(currentSeason == GameTimeStamp.Season.Winter);
    }
}
