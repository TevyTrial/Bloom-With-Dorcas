using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public static int Money {get; private set;}
    public static int Stamina { get; private set; }
    public static int MaxStamina { get; private set; } = 50; // Default max stamina

    public const string CURRENCY = "$";

    // Initialize stamina at game start
    public static void Initialize(int startingMoney, int startingStamina, int maxStamina)
    {
        Money = startingMoney;
        Stamina = startingStamina;
        MaxStamina = maxStamina;
        
        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RenderPlayerStats();
            UIManager.Instance.UpdateStaminaBar(Stamina, MaxStamina);
        }
    }

    public static bool Spend(int cost) {
        //Check if enough money
        if(cost > Money) {
            Debug.Log("Not enough money!");
            return false;
        }
        Money -= cost;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RenderPlayerStats();
        }
        AudioManager.Instance.PlaySpendSFX();
        return true;
    }

    public static void Earn(int income) {
        Money += income;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RenderPlayerStats();
        }
        AudioManager.Instance.PlayReciveSFX();
    }

    public static void UseStamina(int staminaLost) {
        Stamina -= staminaLost;
        
        // Clamp to 0 minimum
        if (Stamina < 0) Stamina = 0;
        
        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStaminaBar(Stamina, MaxStamina);
        }
        
        Debug.Log($"[PlayerStats] Used {staminaLost} stamina. Current: {Stamina}/{MaxStamina}");

        if (Stamina == 0)
        {
            Debug.Log("[PlayerStats] Stamina depleted!");
            //Force the player to rest or take action due to exhaustion
            if(TimeManager.Instance != null) {
                TimeManager.Instance.ForceRest(2); // Force rest for 2 days
            }
            // Don't restore stamina here - it will be done after the video plays
        } else if (Stamina <= 10) {
            UIManager.Instance.ShowTip("You are exhausted! Find a place to rest soon.");
            UIManager.Instance.HideTipAfterDelay(1.5f); // Hide after 1.5 seconds
            
        } else{
            UIManager.Instance.HideTip();
        }
    }

    public static void RecoverStamina(int staminaGained) {
        Stamina += staminaGained;
        
        // Clamp to max
        if (Stamina > MaxStamina) Stamina = MaxStamina;
        
        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStaminaBar(Stamina, MaxStamina);
        }
        
        Debug.Log($"[PlayerStats] Recovered {staminaGained} stamina. Current: {Stamina}/{MaxStamina}");


    }

    // Fully restore stamina (e.g., after sleeping)
    public static void RestoreStaminaFully()
    {
        Stamina = MaxStamina;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStaminaBar(Stamina, MaxStamina);
        }
    }
}