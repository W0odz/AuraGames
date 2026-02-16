using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUIManager : MonoBehaviour
{
    public static CraftingUIManager Instance;

    [Header("Paineis de Exibicao (Topo)")]
    public GameObject resultAndIngredientsGroup; // O grupo que contém o topo e o meio
    public Image resultIcon;
    public TextMeshProUGUI resultName;       
    public TextMeshProUGUI resultDescription;

    [Header("Ingredientes (Meio)")]
    public Transform ingredientsContainer;    // Onde os 4 quadrados aparecem
    public GameObject ingredientPrefab;

    [Header("Lista de Receitas (Baixo)")]
    public Transform recipeListContainer;
    public GameObject recipeButtonPrefab;
    public List<RecipeData> availableRecipes;

    private RecipeData selectedRecipe;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita duplicatas que quebram o sistema
        }
    }

    private void Start()
    {
        // Garante que o painel de detalhes comece invisível
        if (resultAndIngredientsGroup != null)
            resultAndIngredientsGroup.SetActive(false);

        PopulateRecipeList();
    }


    // Cria os botões de ícones na parte de baixo
    public void PopulateRecipeList()
    {
        foreach (Transform child in recipeListContainer) Destroy(child.gameObject);

        foreach (RecipeData receita in availableRecipes)
        {
            if (receita == null) continue;

            GameObject newSlot = Instantiate(recipeButtonPrefab, recipeListContainer);

            // IMPORTANTE: O nome do script aqui deve ser EXATAMENTE o que está no Prefab
            RecipeButtonUI slotScript = newSlot.GetComponent<RecipeButtonUI>();

            if (slotScript != null)
            {
                slotScript.Setup(receita); // É aqui que o botão recebe a receita!
            }
            else
            {
                Debug.LogError("O Prefab do botão está sem o script RecipeButtonUI!");
            }
        }
    }


    // Chamado quando o jogador clica em uma receita na lista inferior
    public void SelectRecipe(RecipeData recipe)
    {
        Debug.Log($"CraftingManager recebeu a receita: {recipe.itemResultado.nomeItem}");

        selectedRecipe = recipe;

        if (resultAndIngredientsGroup != null)
        {
            resultAndIngredientsGroup.SetActive(true);
            Debug.Log("Painel de detalhes ativado com sucesso.");
        }
        else
        {
            Debug.LogError("Erro: O campo 'Result And Ingredient Panel' está VAZIO no Inspector!");
        }

        UpdateIngredientList();
    }

    private void UpdateIngredientList()
    {
        // 1. Limpa os ingredientes da receita selecionada anteriormente
        if (ingredientsContainer == null) return;

        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }

        if (selectedRecipe == null) return;

        // 2. Cria os novos slots de ingredientes para a receita atual
        foreach (var ingrediente in selectedRecipe.ingredientes)
        {
            GameObject newIng = Instantiate(ingredientPrefab, ingredientsContainer);

            // Aqui precisaremos de um script no slot de ingrediente para mostrar o ícone e a quantidade
            IngredientSlotUI slotScript = newIng.GetComponent<IngredientSlotUI>();
            if (slotScript != null)
            {
                slotScript.Setup(ingrediente.item, ingrediente.quantidade);
            }
        }
    }

    public void CraftItem()
    {
        if (selectedRecipe == null) return;

        // 1. Verifica se o jogador tem todos os ingredientes necessários
        foreach (var ing in selectedRecipe.ingredientes)
        {
            if (!HasIngredients(ing.item, ing.quantidade))
            {
                Debug.Log("Faltam materiais para: " + selectedRecipe.itemResultado.nomeItem);
                return;
            }
        }

        // 2. Remove os itens consumidos da mochila
        foreach (var ing in selectedRecipe.ingredientes)
        {
            InventoryManager.Instance.RemoverItem(ing.item, ing.quantidade);
        }

        // 3. Adiciona o item pronto ao inventário
        InventoryManager.Instance.AdicionarItem(selectedRecipe.itemResultado, selectedRecipe.quantidadeResultado);

        // 4. Atualiza as interfaces (Mochila e o próprio painel de Crafting)
        InventoryUIManager.Instance.UpdateAll();
        SelectRecipe(selectedRecipe);
    }

    private bool HasIngredients(DadosItem item, int qty)
    {
        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == item && slot.quantidade >= qty) return true;
        }
        return false;
    }
}