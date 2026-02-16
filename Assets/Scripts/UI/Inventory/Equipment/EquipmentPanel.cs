using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    public EquipmentSlotUI[] equipmentSlots;

    private void OnEnable()
    {
        UpdateUI();
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        // Atualiza cada slot individualmente baseado no novo array
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (i < EquipmentManager.Instance.currentEquipment.Length)
            {
                equipmentSlots[i].UpdateVisual();
            }
        }

        // Avisa o Gerenciador de UI para atualizar os bônus do rodapé
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.UpdateAll();
        }
    }
}