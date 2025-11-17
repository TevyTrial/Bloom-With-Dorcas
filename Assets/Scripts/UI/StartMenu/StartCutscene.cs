using UnityEngine;
using System.Collections;

public class StartCutscene : MonoBehaviour
{
    [SerializeField] private AnimationClip cutsceneClip;
    [SerializeField] private GameObject cameraObject;

    void Start()
    {
        if (cameraObject == null)
            cameraObject = gameObject;
        
        if (cutsceneClip != null)
        {
            StartCoroutine(DisableCameraAfterAnimation());
        }
    }

    private IEnumerator DisableCameraAfterAnimation()
    {
        yield return new WaitForSeconds(cutsceneClip.length);
        cameraObject.SetActive(false);
    }
}