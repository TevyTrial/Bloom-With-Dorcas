using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    // send the message to ancestor objects
    public void NotifyAncestor(string message)
    {
        SendMessageUpwards(message);
    }
}
