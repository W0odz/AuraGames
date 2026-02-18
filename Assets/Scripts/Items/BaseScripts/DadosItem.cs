using System.Collections.Generic;
using UnityEngine;

public enum TipoItem { Consumivel, Equipamento, Material, Chave }
public enum TipoAtaque { Perfurante, Cortante }

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/DadosItem")]
public class DadosItem : ScriptableObject
{
    public string nomeItem;

    [TextArea] public string descricao;
    public Sprite iconeItem;
    public int valorOuro;
    public TipoItem tipoItem;
    public SlotEquipamento slotOndeEquipa;
    public List<ItemEffect> efeitosDoItem;

    [Header("Bônus de Atributos")]
    public int bonusStrength;
    public int bonusResistance;
    public int bonusMaxHP;
    public int bonusMaxMP;

    public void Use(GameObject player)
    {
        if (efeitosDoItem == null || efeitosDoItem.Count == 0) return;

        // Percorre todos os efeitos da lista e executa um por um
        foreach (var efeito in efeitosDoItem)
        {
            if (efeito != null)
            {
                efeito.Execute(player);
            }
        }
    }

    public virtual void ExecutarLogicaEspecial() { }
}