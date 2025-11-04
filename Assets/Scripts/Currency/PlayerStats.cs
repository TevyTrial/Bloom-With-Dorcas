using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money {get; private set;}

    public const string CURRENCY = "$";

    public static void Spend(int cost) {
        //Check if enough money
        if(cost > Money) {
            Debug.Log("Not enough money!");
            return;
        }
        Money -= cost;
        UIManager.Instance.RenderPlayerStats();
    }

    public static void Earn(int income) {
        Money += income;
        UIManager.Instance.RenderPlayerStats();
    }
}
