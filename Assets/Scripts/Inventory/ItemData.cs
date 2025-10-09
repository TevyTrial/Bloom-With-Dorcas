using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string description;

    //item icon
    public Sprite icon;

    //gameobject to be shown in the screen
    public GameObject gameModel;
}
