using UnityEngine;

[CreateAssetMenu(fileName = "DevSword", menuName = "Inventario/Itens Especiais/DevSword")]
public class DevSwordItem : DadosItem
{
    public override void ExecutarLogicaEspecial()
    {
        Debug.Log("<color=cyan>GOD MODE ATIVADO!</color>");
        // Aqui você chamaria seu GameManager ou Status do Player
        // Ex: GameManager.Instance.HPAtual = novoHP;
    }
}