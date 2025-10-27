using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public InventoryBox inventoryBox; // Reference to the InventoryBox component
    public Image itemIcon;
    public CanvasGroup canvasGroup;
    
    [Header("Drag Settings")]
    public float dragAlpha = 0.6f;
    public float returnSpeed = 10f;
    public bool isEquippedSlot = false; // NEW: Mark if this is an equipped slot
    
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
        ItemSlotData slot = null;
        
        // Get slot data based on whether this is an equipped slot or inventory slot
        if (isEquippedSlot)
        {
            slot = InventoryManager.Instance.GetEquippedSlot(inventoryBox.boxType);
            if (slot == null || slot.IsEmpty())
            {
                eventData.pointerDrag = null;
                return;
            }
            draggedIndex = -1; // Use -1 to indicate equipped slot
        }
        else
        {
            slot = inventoryBox.GetSlotData();
            if (slot == null || slot.IsEmpty())
            {
                eventData.pointerDrag = null;
                return;
            }
            draggedIndex = inventoryBox.BoxIndex;
        }

        isDragging = true;
        draggedSlot = new ItemSlotData(slot);
        draggedType = inventoryBox.boxType;
        
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        CreateDragVisual();
        
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragVisual == null) return;

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

        if (dragVisual != null)
        {
            Destroy(dragVisual);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        InventorySlotDragHandler targetHandler = dropTarget?.GetComponentInParent<InventorySlotDragHandler>();

        if (targetHandler == null || !targetHandler.IsValidDropTarget(draggedType))
        {
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

        if (!IsValidDropTarget(dragHandler.draggedType))
        {
            return;
        }

        PerformDrop(dragHandler);
    }

    private void CreateDragVisual()
    {
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
        return inventoryBox.boxType == dragType;
    }

    private void PerformDrop(InventorySlotDragHandler sourceHandler)
    {
        ItemSlotData sourceSlot = sourceHandler.draggedSlot;
        int sourceIndex = sourceHandler.draggedIndex;
        InventoryBox.InventoryType boxType = inventoryBox.boxType;

        // Handle drop to equipped slot
        if (isEquippedSlot)
        {
            if (sourceHandler.isEquippedSlot)
            {
                // Can't drag equipped to equipped of same type
                return;
            }
            
            // Use InventoryToHand to equip the item
            InventoryManager.Instance.InventoryToHand(sourceIndex, boxType);
        }
        // Handle drop from equipped slot to inventory
        else if (sourceHandler.isEquippedSlot)
        {
            ItemSlotData targetSlot = inventoryBox.GetSlotData();
            ItemSlotData[] inventoryArr = InventoryManager.Instance.GetInventorySlots(boxType);
            ItemSlotData equippedSlot = InventoryManager.Instance.GetEquippedSlot(boxType);
            int targetIndex = inventoryBox.BoxIndex;

            if (targetSlot.IsEmpty())
            {
                // Move equipped to empty inventory slot
                inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
                equippedSlot.Empty();
            }
            else if (targetSlot.Stackable(sourceSlot))
            {
                // Stack equipped with inventory
                inventoryArr[targetIndex].AddQuantity(sourceSlot.quantity);
                equippedSlot.Empty();
            }
            else
            {
                // Swap equipped with inventory
                ItemSlotData temp = new ItemSlotData(inventoryArr[targetIndex]);
                inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
                InventoryManager.Instance.EquipHandSlot(temp);
            }

            InventoryManager.Instance.RenderEquippedItem();
        }
        // Handle drop between inventory slots
        else
        {
            ItemSlotData targetSlot = inventoryBox.GetSlotData();
            ItemSlotData[] inventoryArr = InventoryManager.Instance.GetInventorySlots(boxType);
            int targetIndex = inventoryBox.BoxIndex;

            // PROTECTION: Don't allow dragging to same slot
            if (sourceIndex == targetIndex)
            {
                return;
            }

            if (targetSlot.IsEmpty())
            {
                inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
                inventoryArr[sourceIndex].Empty();
            }
            else if (targetSlot.Stackable(sourceSlot))
            {
                inventoryArr[targetIndex].AddQuantity(sourceSlot.quantity);
                inventoryArr[sourceIndex].Empty();
            }
            else
            {
                ItemSlotData temp = new ItemSlotData(inventoryArr[targetIndex]);
                inventoryArr[targetIndex] = new ItemSlotData(sourceSlot);
                inventoryArr[sourceIndex] = temp;
            }
        }

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
        InventorySlotDragHandler dragHandler = eventData.pointerDrag?.GetComponent<InventorySlotDragHandler>();
        
        if (dragHandler != null && dragHandler.isDragging)
        {
            bool isValid = IsValidDropTarget(dragHandler.draggedType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }


}