using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Nova Receita", menuName = "Inventario/Receita")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public struct Ingrediente
    {
        public DadosItem item;
        public int quantidade;
    }

    public List<Ingrediente> ingredientes;
    public DadosItem itemResultado;
    public int quantidadeResultado = 1;
}