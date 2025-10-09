using UnityEngine;

[CreateAssetMenu(menuName = "Items/Seed")]
public class SeedData : ItemData
{   
    //the plant that this seed will grow into
    public ItemData plant;

    //how many days it takes to grow
    public int growTimeInDays;
    
}
