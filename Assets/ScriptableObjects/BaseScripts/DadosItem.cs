using UnityEngine;

public enum TipoItem { Consumivel, Equipamento, Material, Chave }

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/DadosItem")]
public class DadosItem : ScriptableObject
{
    public string nomeItem;
    [TextArea] public string descricao;
    public Sprite iconeItem;
    public int valorOuro;
    public TipoItem tipoItem;
    public SlotEquipamento slotOndeEquipa;

    [Header("Bônus de Atributos")]
    public int bonusStrength;
    public int bonusResistance;
    public int bonusMaxHP;
    public int bonusMaxMP;

    public virtual void ExecutarLogicaEspecial() { }
}