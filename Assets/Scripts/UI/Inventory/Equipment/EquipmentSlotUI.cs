using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    public SlotEquipamento slotType; // Define se é Helmet, Weapon, etc.
    public Image iconImage;         // A imagem que está ficando branca

    private void OnEnable()
    {
        UpdateVisual();
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += UpdateVisual;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= UpdateVisual;
    }

    public void UpdateVisual()
    {
        if (EquipmentManager.Instance == null) return;

        // Pega o item atual do array baseado no tipo deste slot
        DadosItem item = EquipmentManager.Instance.currentEquipment[(int)slotType];

        if (item != null && item.iconeItem != null)
        {
            iconImage.sprite = item.iconeItem; //
            iconImage.enabled = true;          // Mostra o ícone
            iconImage.color = Color.white;
        }
        else
        {
            // SOLUÇÃO PARA O QUADRADO BRANCO
            iconImage.enabled = false;         // Esconde a imagem se não houver item
            iconImage.sprite = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EquipmentManager.Instance == null) return;

        // Slot de arma não pode ser desequipado clicando — só trocando por outra arma
        if (slotType == SlotEquipamento.Weapon)
        {
            Debug.LogWarning("[EquipmentSlotUI] A arma não pode ser removida. Equipe outra arma para substituir.");
            return;
        }

        EquipmentManager.Instance.Unequip((int)slotType);

        // Esconde o tooltip ao desequipar para não bugar
        if (TooltipManager.Instance != null) TooltipManager.Instance.Hide();
    }
}