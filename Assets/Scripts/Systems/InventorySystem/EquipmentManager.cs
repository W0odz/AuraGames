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

    public bool IsItemEquipped(Item item)
    {
        if (item == null) return false;

        // Percorre os slots de equipamento para ver se o item está lá
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null && currentEquipment[i].itemName == item.itemName)
            {
                return true;
            }
        }
        return false;
    }

    public void Equip(EquipmentItem newItem)
    {
        if (newItem == null) return;

        int slotIndex = (int)newItem.equipSlot;

        // LOG DE TESTE 1: Entrou na função?
        Debug.Log($"[Manager] Tentando equipar: {newItem.itemName} no slot índice: {slotIndex}");

        EquipmentItem oldItem = currentEquipment[slotIndex];

        // Se clicar no mesmo item, desequipa
        if (oldItem == newItem)
        {
            currentEquipment[slotIndex] = null;
            Debug.Log("[Manager] Item igual detectado. Desequipando...");
        }
        else
        {
            // Salva o item no array
            currentEquipment[slotIndex] = newItem;
            Debug.Log($"[Manager] {newItem.itemName} salvo com sucesso no array de equipamentos!");
        }

        // LOG DE TESTE 2: O array realmente contém o item agora?
        if (currentEquipment[slotIndex] != null)
        {
            Debug.Log($"[Manager] Verificação imediata: Slot {slotIndex} agora contém {currentEquipment[slotIndex].itemName}");
        }

        // INVOCA OS EVENTOS (Isso faz a UI acordar)
        onEquipmentChanged?.Invoke(newItem, oldItem);

        if (InventoryManager.Instance != null)
        {
            Debug.Log("[Manager] Avisando UI para atualizar os indicadores...");
            InventoryManager.Instance.onInventoryChangedCallback?.Invoke();
        }
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            EquipmentItem oldItem = currentEquipment[slotIndex];
            currentEquipment[slotIndex] = null;

            // Trigger para avisar que desequipou
            onEquipmentChanged?.Invoke(null, oldItem);
            InventoryManager.Instance.onInventoryChangedCallback?.Invoke();
            Debug.Log($"Unequipped {oldItem.itemName}");
        }
    }
}