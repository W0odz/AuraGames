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
        if (recipe == null) return;

        selectedRecipe = recipe;

        // 1. ATRIBUIÇÃO DOS DADOS DO RESULTADO
        // Usamos os dados do 'itemResultado' que está dentro da receita
        if (recipe.itemResultado != null)
        {
            resultName.text = recipe.itemResultado.nomeItem;
            resultDescription.text = recipe.itemResultado.descricao;
            resultIcon.sprite = recipe.itemResultado.iconeItem;
        }

        // 2. ATIVA O PAINEL (Caso ele comece escondido)
        if (resultAndIngredientsGroup != null)
        {
            resultAndIngredientsGroup.SetActive(true);
        }

        // 3. ATUALIZA OS INGREDIENTES (Função que criamos antes)
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
        // 1. Segurança: Verifica se há uma receita selecionada
        if (selectedRecipe == null) return;

        // 2. Verifica se o jogador REALMENTE tem os itens (Double check)
        if (!CanCraft(selectedRecipe))
        {
            Debug.Log("Você não tem materiais suficientes!");
            return;
        }

        // 3. CONSOME os ingredientes do inventário
        foreach (var ing in selectedRecipe.ingredientes)
        {
            InventoryManager.Instance.RemoverItem(ing.item, ing.quantidade);
        }

        // 4. ADICIONA o item criado (ex: o Ensopado)
        InventoryManager.Instance.AdicionarItem(selectedRecipe.itemResultado, 1);

        // 5. ATUALIZA as UIs para o jogador ver a mudança na hora
        UpdateIngredientList(); // Atualiza os números (ex: de 12/3 para 9/3)
        InventoryUIManager.Instance.UpdateGrid(); // Atualiza o grid da mochila

        Debug.Log($"{selectedRecipe.itemResultado.nomeItem} criado com sucesso!");
    }

    // Função auxiliar para checar se o craft é possível
    private bool CanCraft(RecipeData recipe)
    {
        foreach (var ing in recipe.ingredientes)
        {
            if (InventoryManager.Instance.GetItemCount(ing.item) < ing.quantidade)
                return false;
        }
        return true;
    }

    private bool HasIngredients(DadosItem item, int qty)
    {
        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == item && slot.quantidade >= qty) return true;
        }
        return false;
    }

    public void ResetCraftingUI()
    {
        // 1. Reseta a variável da receita atual
        selectedRecipe = null;

        // 2. Esconde o painel de detalhes e resultados
        if (resultAndIngredientsGroup != null)
        {
            resultAndIngredientsGroup.SetActive(false);
        }

        // 3. Limpa visualmente os ingredientes do meio
        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}