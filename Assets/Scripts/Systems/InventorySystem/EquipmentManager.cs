using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    // Array to hold equipped items based on the EquipmentSlot enum
    public EquipmentItem[] currentEquipment;

    // Event to notify UI or Stats system
    public delegate void OnEquipmentChanged(EquipmentItem newItem, EquipmentItem oldItem);
    public OnEquipmentChanged onEquipmentChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize the array based on the number of slots in the enum
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new EquipmentItem[numSlots];
    }

    public void Equip(EquipmentItem newItem)
    {
        int slotIndex = (int)newItem.equipSlot;

        EquipmentItem oldItem = null;

        // If there's already an item in that slot, unequip it first
        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
            InventoryManager.Instance.AddItem(oldItem); // Return to inventory
        }

        // Trigger the callback for stats update
        onEquipmentChanged?.Invoke(newItem, oldItem);

        currentEquipment[slotIndex] = newItem;
        Debug.Log($"Equipped {newItem.itemName} in slot {newItem.equipSlot}");
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            EquipmentItem oldItem = currentEquipment[slotIndex];
            InventoryManager.Instance.AddItem(oldItem);

            currentEquipment[slotIndex] = null;

            onEquipmentChanged?.Invoke(null, oldItem);
        }
    }
}