using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string description;

    //item icon
    public Sprite icon;

    [Header("Models")]
    //gameobject to be shown in the player's hand
    public GameObject onHandModel;

    [Header("Item Properties")]
    public int cost;
    
}
