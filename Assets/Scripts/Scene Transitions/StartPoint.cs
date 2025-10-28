using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct StartPoint
{
    // Location the player is entering 
    public SceneTransitionManager.Location enteringFrom;

    // The transform the player should start at
    public Transform playerStart;
}
