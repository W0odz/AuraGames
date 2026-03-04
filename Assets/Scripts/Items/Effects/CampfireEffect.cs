using UnityEngine;

/// <summary>
/// Efeito do item Fogueira.
/// Quando executado (uso do item):
///   1. Salva o jogo em disco (via GameManager.SaveCurrentGame)
///   2. Restaura a Força de Vontade do jogador
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

        // 1. Salvar o jogo usando o sistema existente
        if (GameManager.Instance != null)
            GameManager.Instance.SaveCurrentGame();

        // 2. Restaurar a Força de Vontade
        player.RestaurarForcaDeVontade();

        Debug.Log("[Fogueira] Jogo salvo e Força de Vontade restaurada!");
    }
}