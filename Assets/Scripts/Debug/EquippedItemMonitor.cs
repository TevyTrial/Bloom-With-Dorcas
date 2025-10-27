using UnityEngine;

/// <summary>
/// Debug monitor to track equipped item changes and help find bugs
/// Attach this to a GameObject in the scene to monitor equipped slots
/// </summary>
public class EquippedItemMonitor : MonoBehaviour
{
    [Header("Monitoring Settings")]
    public bool enableLogging = true;
    public float checkInterval = 0.5f; // Check every 0.5 seconds
    
    private ItemData lastEquippedTool = null;
    private ItemData lastEquippedItem = null;
    private float nextCheckTime = 0f;

    void Update()
    {
        if (!enableLogging || Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;

        if (InventoryManager.Instance == null)
            return;

        // Check tool slot
        ItemSlotData toolSlot = InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Tool);
        ItemData currentTool = (toolSlot != null && !toolSlot.IsEmpty()) ? toolSlot.itemData : null;

        if (currentTool != lastEquippedTool)
        {
            string oldName = lastEquippedTool != null ? lastEquippedTool.name : "null";
            string newName = currentTool != null ? currentTool.name : "null";
            Debug.LogWarning($"[EquippedMonitor] TOOL SLOT CHANGED: '{oldName}' -> '{newName}'\nStack Trace:\n{System.Environment.StackTrace}");
            lastEquippedTool = currentTool;
        }

        // Check item slot
        ItemSlotData itemSlot = InventoryManager.Instance.GetEquippedSlot(InventoryBox.InventoryType.Item);
        ItemData currentItem = (itemSlot != null && !itemSlot.IsEmpty()) ? itemSlot.itemData : null;

        if (currentItem != lastEquippedItem)
        {
            string oldName = lastEquippedItem != null ? lastEquippedItem.name : "null";
            string newName = currentItem != null ? currentItem.name : "null";
            Debug.LogWarning($"[EquippedMonitor] ITEM SLOT CHANGED: '{oldName}' -> '{newName}'\nStack Trace:\n{System.Environment.StackTrace}");
            lastEquippedItem = currentItem;
        }
    }

    void OnGUI()
    {
        if (!enableLogging)
            return;

        // Display current equipped items on screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("=== EQUIPPED MONITOR ===");
        
        string toolName = lastEquippedTool != null ? lastEquippedTool.name : "NONE";
        string itemName = lastEquippedItem != null ? lastEquippedItem.name : "NONE";
        
        GUILayout.Label($"Equipped Tool: {toolName}");
        GUILayout.Label($"Equipped Item: {itemName}");
        GUILayout.EndArea();
    }
}
