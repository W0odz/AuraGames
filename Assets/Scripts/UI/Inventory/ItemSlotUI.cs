using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Referências Visuais")]
    public Image iconeItem;
    public TextMeshProUGUI quantidadeTexto; // Apenas para consumíveis/materiais

    private DadosItem itemDados;

    public void ConfigurarSlot(DadosItem item, int quantidade)
    {
        itemDados = item;

        if (iconeItem != null)
        {
            // 1. Garante que o sprite do Scriptable Object vá para a UI
            iconeItem.sprite = item.iconeItem;
            iconeItem.enabled = true;
            iconeItem.color = Color.white; // Remove qualquer transparência de placeholder
        }

        if (quantidadeTexto != null)
        {
            // Mostra a quantidade apenas se for maior que 1 (estética de RPG)
            quantidadeTexto.text = quantidade > 1 ? quantidade.ToString() : "";
        }

        // CONFIGURAÇÃO DO CLIQUE (Para tornar clicável)
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(AoClicar);
        }
    }

    public void AoClicar()
    {
        if (itemDados != null)
        {
            InventoryUIManager.Instance.AoClicarNoIcone(itemDados);
        }
    }
}