using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI amountText;
    private DadosItem linkedItem;

    // Configura o item e a quantidade no slot
    public void Setup(DadosItem item, int qty)
    {
        linkedItem = item;

        if (item != null)
        {
            iconImage.sprite = item.iconeItem;
            iconImage.enabled = true;

            if (amountText != null)
            {
                if (qty > 1)
                {
                    amountText.text = qty.ToString();
                    amountText.enabled = true;

                    // --- LÓGICA DE AJUSTE DE TAMANHO ---
                    amountText.enableAutoSizing = true; // Ativa o redimensionamento automático
                    amountText.fontSizeMin = 12;        // Tamanho mínimo (para não ficar ilegível)
                    amountText.fontSizeMax = 24;        // Tamanho máximo (o padrão do seu slot)
                }
                else
                {
                    amountText.enabled = false;
                }
            }
        }
        else
        {
            iconImage.enabled = false;
            if (amountText != null) amountText.enabled = false;
        }
    }

    public DadosItem GetItem()
    {
        return linkedItem;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Só mostra se o item existir e o Manager estiver pronto
        if (linkedItem != null && TooltipManager.Instance != null)
        {
            // Passa a posição do transform para o cálculo "abaixo do slot"
            TooltipManager.Instance.Show(linkedItem.nomeItem, linkedItem.descricao, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // PROTEÇÃO: Verifica se o Singleton existe antes de chamar
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.Hide();
        }
    }

    public void OnClick()
    {
        if (linkedItem == null) return;

        // Verifica se o item é do tipo Equipamento
        if (linkedItem.tipoItem == TipoItem.Equipamento)
        {
            Debug.Log("Tentando equipar: " + linkedItem.nomeItem);
            EquipmentManager.Instance.Equip(linkedItem);

            // Esconde o tooltip ao equipar para não travar na tela
            if (TooltipManager.Instance != null) TooltipManager.Instance.Hide();
        }
    }
}