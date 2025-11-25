using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    public ItemData item;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public virtual void Pickup()
    {
        if (item == null)
        {
            Debug.LogWarning($"{name} has no ItemData assigned.");
            return;
        }
        InventoryManager.Instance.EquipHandSlot(item);
        InventoryManager.Instance.RenderEquippedItem();
        Destroy(gameObject);
    }
}