using UnityEngine;

/// <summary>
/// Efeito do item Fogueira.
/// Quando executado (uso do item):
///   - Restaura a Força de Vontade do jogador
/// O jogo é salvo ao retornar ao menu principal pelo painel de pause.
/// </summary>
[CreateAssetMenu(fileName = "Efeito_Fogueira", menuName = "Items/Efeitos/Usar Fogueira")]
public class UsarFogueiraEffect : ItemEffect
{
    public override void Execute(GameObject playerGO)
    {
        PlayerUnit player = PlayerUnit.Instance;

        if (player == null)
        {
            Debug.LogWarning("[UsarFogueiraEffect] PlayerUnit.Instance não encontrado!");
            return;
        }

        // Restaurar a Força de Vontade
        player.RestaurarForcaDeVontade();

        Debug.Log("[Fogueira] Força de Vontade restaurada!");
    }
}