using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ITimeTracker
{
    void ClockUpdate(GameTimeStamp currentTime)
    {
        // Update the UI elements to reflect the current in-game time
       
        // Example: Update a UI text element with currentTime.hour and currentTime.minute
    }
}
