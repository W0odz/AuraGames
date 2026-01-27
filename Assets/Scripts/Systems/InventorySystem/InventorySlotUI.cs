using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject equippedIndicator; // Arraste o quadradinho verde aqui
    private Item currentItem; // The item currently in this slot

    // Called by the InventoryUI to fill the slot with data
    public void AddItem(Item newItem, bool isEquipped)
    {
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.enabled = true;

        // Liga ou desliga o quadradinho verde
        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(isEquipped);
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

        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }
}