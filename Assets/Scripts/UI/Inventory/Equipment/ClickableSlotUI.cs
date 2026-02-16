using UnityEngine;

public class ClickableSlotUI : MonoBehaviour
{
    private ItemSlotUI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<ItemSlotUI>();
    }

    public void OnSlotClicked()
    {
        if (slotUI == null) return;

        // Agora o GetItem() existe no ItemSlotUI e o erro desaparece!
        DadosItem itemInSlot = slotUI.GetItem();

        if (itemInSlot != null && itemInSlot.tipoItem == TipoItem.Equipamento)
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.Equip(itemInSlot);
            }
        }
    }
}