using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bed : MonoBehaviour
{
    private bool canInteract = true;
    private float interactionCooldown = 0.5f; // Half second cooldown

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.ShowSleepingTooltip();
            canInteract = true; // Reset interaction availability
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.ShowSleepingTooltip();
        }

        // Check if player presses Q while in the trigger
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q) && canInteract)
        {
            canInteract = false; // Prevent multiple interactions
            UIManager.Instance.HideSleepingTooltip();
            UIManager.Instance.TriggerYesNoPrompt("Do you want to sleep?", SleepAction);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.HideSleepingTooltip();
            canInteract = true; // Reset when leaving
        }
    }

    private void SleepAction()
    {
        UIManager.Instance.HideSleepingTooltip();
        TimeManager.Instance.Sleep();
        StartCoroutine(ResetInteraction());
    }

    private IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}