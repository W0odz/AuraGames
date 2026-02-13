using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public void TentarCraftar(CraftingRecipe receita)
    {
        // 1. Verificar se tem todos os ingredientes
        foreach (var ing in receita.ingredientes)
        {
            if (!TemQuantidadeSuficiente(ing.item, ing.quantidade))
            {
                Debug.Log("Ingredientes insuficientes!");
                return;
            }
        }

        // 2. Se chegou aqui, tem tudo! Remover ingredientes
        foreach (var ing in receita.ingredientes)
        {
            InventoryManager.Instance.RemoverItem(ing.item, ing.quantidade);
        }

        // 3. Adicionar o resultado
        InventoryManager.Instance.AdicionarItem(receita.itemResultado, receita.quantidadeResultado);

        Debug.Log($"Craftado com sucesso: {receita.itemResultado.nomeItem}");
    }

    bool TemQuantidadeSuficiente(DadosItem item, int qtdNecessaria)
    {
        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == item && slot.quantidade >= qtdNecessaria) return true;
        }
        return false;
    }
}