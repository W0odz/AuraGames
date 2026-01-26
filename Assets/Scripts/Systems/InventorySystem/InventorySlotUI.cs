using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    private Item currentItem; // The item currently in this slot

    // Called by the InventoryUI to fill the slot with data
    public void AddItem(Item newItem)
    {
        if (newItem == null) return;

        currentItem = newItem;

        // Aqui acontece a mágica:
        if (newItem.icon != null)
        {
            iconImage.sprite = newItem.icon;
            iconImage.enabled = true; // Garante que a imagem apareça
        }
        else
        {
            iconImage.enabled = false; // Se o item não tiver ícone, fica invisível
        }
    }

    // Called if the slot needs to be emptied
    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    // Triggered when the button is clicked
    public void OnSlotPressed()
    {
        if (this == null || gameObject == null) return;

        if (currentItem != null)
        {
            Debug.Log($"Usando item: {currentItem.itemName}");
            currentItem.Use();
        }
    }
}