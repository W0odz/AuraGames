using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;

    public void Setup(DadosItem item, int quantidadeNecessaria)
    {
        if (item != null)
        {
            icon.sprite = item.iconeItem;

            // Busca quantos itens o jogador tem na mochila
            int quantidadeNaMochila = InventoryManager.Instance.GetItemCount(item);

            // Exemplo visual: "1 / 3"
            amountText.text = $"{quantidadeNaMochila} / {quantidadeNecessaria}";

            // Muda a cor para vermelho se não tiver o suficiente
            if (quantidadeNaMochila < quantidadeNecessaria)
            {
                amountText.color = Color.red;
            }
            else
            {
                amountText.color = Color.green;
            }
            ;
        }
    }
}