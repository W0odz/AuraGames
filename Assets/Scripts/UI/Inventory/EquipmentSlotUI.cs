using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour
{
    public Image iconeItem;
    public TextMeshProUGUI nomeTexto;
    public TextMeshProUGUI quantidadeItem;

    private DadosItem itemDados;

    public void Configurar(DadosItem item, int quantidade)
    {
        itemDados = item;
        if (nomeTexto != null) nomeTexto.text = item.nomeItem;
        if (quantidadeItem != null) quantidadeItem.text = "x" + quantidade;

        if (iconeItem != null)
        {
            // AQUI ESTÁ O SEGREDO:
            // Substituímos o sprite branco pelo ícone do item.
            iconeItem.sprite = item.iconeItem;

            // Garantimos que está visível e com cor total (sem transparência)
            iconeItem.enabled = true;
            iconeItem.color = Color.white;
        }
    }

    public void ConfigurarNenhum(SlotEquipamento slotParaLimpar, Sprite spritePadrao)
    {
        nomeTexto.text = "Nenhum (Remover)";
        if (iconeItem != null)
        {
            iconeItem.sprite = spritePadrao;
            iconeItem.enabled = true;
            iconeItem.color = new Color(1, 1, 1, 0.5f);
        }

        // RESOLVE O ERRO: Remove qualquer função de "Equipar" que o prefab tenha e coloca "Desequipar"
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners(); // Limpa a lógica de equipar item real
            btn.onClick.AddListener(() => {
                // Chama diretamente a remoção pelo índice do slot
                EquipmentManager.Instance.Desequipar((int)slotParaLimpar);
            });
        }
    }

    public void AoClicar()
    {
        // Ao clicar na fileira da lista lateral, equipa o item
        InventoryUIManager.Instance.AoClicarNoIcone(itemDados);
    }
}