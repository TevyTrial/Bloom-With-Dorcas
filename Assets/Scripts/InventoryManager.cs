using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }
    
    public void Awake() {
        //check if there is already an instance of this object
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        } 
        
        //set the instance to this object
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize arrays here rather than using field initializers
        InitializeInventory();
    }
    
    [Header("Tools")]
    //array to hold the tools
    public ItemData[] tools = new ItemData[10];
    //the currently equipped tool
    public ItemData equippedTool = null;

    [Header("Items")]
    //array to hold the items
    public ItemData[] items = new ItemData[10];
    //the currently selected item
    public ItemData selectedItem = null;
    
    private void InitializeInventory() {
        // Initialize arrays in Awake to avoid threading issues
        tools = new ItemData[10];
        items = new ItemData[10];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
