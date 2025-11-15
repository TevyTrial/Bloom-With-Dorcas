using UnityEngine;

[CreateAssetMenu(menuName = "Items/Food")]
public class Food : ScriptableObject
{
    public string foodName;
    public Sprite icon;
    public int staminaRestore;
    public GameObject foodModel;
}
