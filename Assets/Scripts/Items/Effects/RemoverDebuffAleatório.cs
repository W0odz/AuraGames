using UnityEngine;

[CreateAssetMenu(fileName = "Efeito_RemoverDebuff", menuName = "Items/Efeitos/Remover Debuff Aleatório")]
public class RemoverDebuffAleatorioEffect : ItemEffect
{
    public override void Execute(GameObject playerGO)
    {
        PlayerUnit player = PlayerUnit.Instance;
        if (player == null)
        {
            Debug.LogWarning("[RemoverDebuffAleatorioEffect] PlayerUnit.Instance não encontrado!");
            return;
        }

        player.RemoverDebuffAleatorio();
    }
}