using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
    public Image itemIcon;

    private RecipeData currentRecipe;

    public void Setup(RecipeData recipe)
    {
        if (recipe.itemResultado != null) // Mudado de resultItem para itemResultado
        {
            itemIcon.sprite = recipe.itemResultado.iconeItem;
            currentRecipe = recipe;
        }
    }

    public void OnClick()
    {
        // 1. Verifica se o Manager existe na cena
        if (CraftingUIManager.Instance == null)
        {
            Debug.LogError("ERRO: O CraftingUIManager não foi encontrado na cena ou não está ativo!");
            return;
        }

        // 2. Verifica se este botão recebeu uma receita no Setup
        if (currentRecipe == null)
        {
            Debug.LogError("ERRO: Este botão não tem uma receita atribuída!");
            return;
        }

        // 3. Se tudo estiver OK, chama o painel
        CraftingUIManager.Instance.SelectRecipe(currentRecipe);
    }
}