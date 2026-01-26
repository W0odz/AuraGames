using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlot> inventory = new List<InventorySlot>();
    public int slotLimit = 20;

    // Evento para avisar a UI que o inventário mudou
    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool HasItem(Item itemToCheck)
    {
        // Search the list for a slot that contains the specific item
        return inventory.Exists(slot => slot.item == itemToCheck);
    }

    public bool AddItem(Item newItem, int quantity = 1)
    {
        // 1. Tentar empilhar se o item for acumulável
        if (newItem.stackable)
        {
            InventorySlot slotExistente = inventory.Find(s => s.item == newItem && s.quantity < newItem.maxStack);
            
            if (slotExistente != null)
            {
                slotExistente.AddQuantity(quantity);
                onInventoryChangedCallback?.Invoke();
                return true;
            }
        }

        // 2. Se não deu para empilhar, verificar se há espaço para um novo slot
        if (inventory.Count >= slotLimit)
        {
            Debug.Log("Inventário cheio!");
            return false;
        }

        // 3. Adicionar como novo slot
        inventory.Add(new InventorySlot(newItem, quantity));

        if (onInventoryChangedCallback != null)
            onInventoryChangedCallback.Invoke();

        return true;
    }

    public void RemoveItem(Item itemParaRemover)
    {
        InventorySlot slot = inventory.Find(s => s.item == itemParaRemover);
        if (slot != null)
        {
            if (slot.quantity > 1) slot.RemoveQuantity(1);
            else inventory.Remove(slot);
        }
        
        onInventoryChangedCallback?.Invoke();
    }
}