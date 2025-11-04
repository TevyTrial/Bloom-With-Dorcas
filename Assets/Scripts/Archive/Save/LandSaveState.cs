using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LandSaveState
{
    public Land.LandState landStatus;
    public GameTimeStamp lastWatered;

    public LandSaveState(Land.LandState landStatus, GameTimeStamp lastWatered)
    {
        this.landStatus = landStatus;
        this.lastWatered = lastWatered;
    }

}