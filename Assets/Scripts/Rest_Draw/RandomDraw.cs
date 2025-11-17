using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomDraw : MonoBehaviour
{
    private bool canInteract = true;
    private float interactionCooldown = 0.5f; // Half second cooldown

    [SerializeField] private List<Food> foodOptions = new();
    [SerializeField] private Transform drawAnchor;
    [SerializeField] private float drawDelay = 1.5f;
    [SerializeField] private Animator drawAnimator; 
    [SerializeField] private string drawTriggerName = "Draw";

    private GameObject currentFoodInstance;
    private Coroutine drawRoutine;
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.ShowDrawingTooltip();
            canInteract = true; // Reset interaction availability
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Check if player presses F while in the trigger
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {           
            UIManager.Instance.HideDrawingTooltip();
            UIManager.Instance.TriggerYesNoPrompt("Do you want to get food from the pond Cost $10?", DrawFoodAction);
        }
    }

    private void DrawFoodAction()
    {
        PlayerStats.Spend(10); // Spend 10 money to draw food
        if (foodOptions == null || foodOptions.Count == 0)
        {
            Debug.LogWarning("[RandomDraw] No food options configured.");
            StartCoroutine(ResetInteraction());
            return;
        }

        if (drawRoutine != null)
        {
            StopCoroutine(drawRoutine);
        }

        drawRoutine = StartCoroutine(DrawFoodRoutine());
    }

    private IEnumerator DrawFoodRoutine()
    {
        if (drawAnimator != null && !string.IsNullOrEmpty(drawTriggerName))
        {
            drawAnimator.SetTrigger(drawTriggerName);
        }

        yield return new WaitForSeconds(drawDelay);

        var selectedFood = foodOptions[Random.Range(0, foodOptions.Count)];

        if (currentFoodInstance != null)
        {
            Destroy(currentFoodInstance);
        }

        if (drawAnchor != null && selectedFood.foodModel != null)
        {
            currentFoodInstance = Instantiate(selectedFood.foodModel, drawAnchor);
            currentFoodInstance.transform.localPosition = Vector3.zero;
            currentFoodInstance.transform.localRotation = Quaternion.identity;
        }

        PlayerStats.RecoverStamina(selectedFood.staminaRestore);
        Debug.Log($"[RandomDraw] Player drew {selectedFood.foodName} and recovered {selectedFood.staminaRestore} stamina.");

        UIManager.Instance.ShowTip($"You get {selectedFood.foodName} from the pond and recovered {selectedFood.staminaRestore} stamina!");


        // Wait for food display duration before destroying
        yield return new WaitForSeconds(5f);

        if (currentFoodInstance != null)
        {
            Destroy(currentFoodInstance);
            currentFoodInstance = null;
        }

        UIManager.Instance.HideTip();

        yield return ResetInteraction();
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.HideDrawingTooltip();
            canInteract = true; // Reset when leaving
        }
    }

    private IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}