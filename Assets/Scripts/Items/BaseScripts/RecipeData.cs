using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Nova Receita", menuName = "Crafting/Receita")]
public class RecipeData : ScriptableObject
{
    // Nomes em português para bater com seus scripts de Sistema e UI
    public DadosItem itemResultado;
    public int quantidadeResultado = 1;

    [System.Serializable]
    public struct Ingrediente
    {
        public DadosItem item;
        public int quantidade;
    }

    public List<Ingrediente> ingredientes;
}