using UnityEngine;

/// <summary>
/// Coloque este componente em qualquer GameObject da cena de exploração.
/// Define o sprite de fundo que será exibido na BattleScene ao entrar em
/// combate a partir desta cena.
/// 
/// O fundo é lido por BattleScene via GameManager.Instance.battleBackground.
/// </summary>
public class BattleBackgroundSetter : MonoBehaviour
{
    [Tooltip("Sprite do fundo de batalha desta cena. Será exibido na BattleScene.")]
    public Sprite fundoDeBatalha;

    private void Start()
    {
        if (GameManager.Instance != null && fundoDeBatalha != null)
            GameManager.Instance.battleBackground = fundoDeBatalha;
    }
}
