using UnityEngine;

/// <summary>
/// Coloque este componente em qualquer GameObject da cena de exploração.
/// Define qual fundo será usado na BattleScene quando o combate iniciar desta cena.
/// </summary>
public class BattleBackgroundSetter : MonoBehaviour
{
    [Tooltip("Sprite que será usado como fundo na BattleScene.")]
    public Sprite fundo;

    private void Awake()
    {
        if (GameManager.Instance != null && fundo != null)
            GameManager.Instance.battleBackground = fundo;
    }
}