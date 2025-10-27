using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    public InventoryBox inventoryBox; // Reference to the InventoryBox component
    public Image itemIcon;
    public CanvasGroup canvasGroup;
    
    [Header("Drag Settings")]
    public float dragAlpha = 0.6f;
    public float returnSpeed = 10f;
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;
    private GameObject dragVisual;
    private ItemSlotData draggedSlot;
    private int draggedIndex;
    private InventoryBox.InventoryType draggedType;
    private bool isDragging = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if there's an item in this slot
        ItemSlotData slot = inventoryBox.GetSlotData();
        if (slot == null || slot.IsEmpty())
        {
            eventData.pointerDrag = null; // Cancel drag
            return;
        }

        isDragging = true;
        draggedSlot = new ItemSlotData(slot);
        draggedIndex = inventoryBox.BoxIndex;
        draggedType = inventoryBox.boxType;
        
        // Store original position
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Create drag visual
        CreateDragVisual();
        
        // Make original slot semi-transparent
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;

        Debug.Log($"[DragHandler] Begin drag: {draggedSlot.itemData.name} from {draggedType} slot {draggedIndex}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragVisual == null) return;

        // Move drag visual to follow cursor
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out position
        );
        
        dragVisual.transform.position = canvas.transform.TransformPoint(position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Clean up
        if (dragVisual != null)
        {
            Destroy(dragVisual);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if dropped on a valid target
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        InventorySlotDragHandler targetHandler = dropTarget?.GetComponentInParent<InventorySlotDragHandler>();

        if (targetHandler == null || !targetHandler.IsValidDropTarget(draggedType))
        {
            // Invalid drop - return to original position
            Debug.Log("[DragHandler] Invalid drop target - returning to original position");
            StartCoroutine(ReturnToOriginalPosition());
        }

        isDragging = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotDragHandler dragHandler = eventData.pointerDrag?.GetComponent<InventorySlotDragHandler>();
        
        if (dragHandler == null || !dragHandler.isDragging)
        {
            return;
        }

        // Validate drop
        if (!IsValidDropTarget(dragHandler.draggedType))
        {
            Debug.Log($"[DragHandler] Invalid drop: Cannot drop {dragHandler.draggedType} into {inventoryBox.boxType} slot");
            return;
        }

        // Perform the swap/stack operation
        PerformDrop(dragHandler);
    }

    private void CreateDragVisual()
    {
        // Create a visual representation that follows the cursor
        dragVisual = new GameObject("DragVisual");
        dragVisual.transform.SetParent(canvas.transform, false);
        dragVisual.transform.SetAsLastSibling();

        RectTransform visualRect = dragVisual.AddComponent<RectTransform>();
        visualRect.sizeDelta = rectTransform.sizeDelta;

        Image visualImage = dragVisual.AddComponent<Image>();
        visualImage.sprite = draggedSlot.itemData.icon;
        visualImage.raycastTarget = false;

        CanvasGroup visualGroup = dragVisual.AddComponent<CanvasGroup>();
        visualGroup.alpha = 0.8f;
        visualGroup.blocksRaycasts = false;
    }

    private bool IsValidDropTarget(InventoryBox.InventoryType dragType)
    {
        // Can only drop tools in tool slots and items in item slots
        return inventoryBox.boxType == dragType;
    }

    private void PerformDrop(InventorySlotDragHandler sourceHandler)
    {
        ItemSlotData sourceSlot = sourceHandler.draggedSlot;
        ItemSlotData targetSlot = inventoryBox.GetSlotData();

        int sourceIndex = sourceHandler.draggedIndex;
        int targetIndex = inventoryBox.BoxIndex;
        InventoryBox.InventoryType boxType = inventoryBox.boxType;

        Debug.Log($"[DragHandler] Performing drop: {sourceSlot.itemData.name} -> slot {targetIndex}");

        // Get the inventory array
        ItemSlotData[] inventoryArr = InventoryManager.Instance.GetInventorySlots(boxType);

        // Check if target is empty
        if (targetSlot.IsEmpty())
        {
            // Move to empty slot
            inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
            inventoryArr[sourceIndex].Empty();
            Debug.Log("[DragHandler] Moved to empty slot");
        }
        // Check if stackable
        else if (targetSlot.Stackable(sourceSlot))
        {
            // Stack items
            inventoryArr[targetIndex].AddQuantity(sourceSlot.quantity);
            inventoryArr[sourceIndex].Empty();
            Debug.Log("[DragHandler] Stacked items");
        }
        else
        {
            // Swap items
            ItemSlotData temp = new ItemSlotData(inventoryArr[targetIndex]);
            inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
            inventoryArr[sourceIndex] = temp;
            Debug.Log("[DragHandler] Swapped items");
        }

        // Update UI
        UIManager.Instance.RenderInventory();
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, elapsed * returnSpeed);
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight valid/invalid drop zones during drag
        InventorySlotDragHandler dragHandler = eventData.pointerDrag?.GetComponent<InventorySlotDragHandler>();
        
        if (dragHandler != null && dragHandler.isDragging)
        {
            bool isValid = IsValidDropTarget(dragHandler.draggedType);
            HighlightSlot(isValid);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RemoveHighlight();
    }

    private void HighlightSlot(bool isValid)
    {
        if (itemIcon != null)
        {
            itemIcon.color = isValid ? new Color(0.5f, 1f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);
        }
    }

    private void RemoveHighlight()
    {
        if (itemIcon != null)
        {
            itemIcon.color = Color.white;
        }
    }
}