using UnityEngine;

[CreateAssetMenu(menuName = "Items/Equipment")]
public class EquipmentData : ItemData
{
    public enum ToolType {
        Hoe, 
        WaterCan,
        Axe,
        PickAxe,
        Rake
    }
    public ToolType toolType;
}
