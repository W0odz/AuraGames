using UnityEngine;

/// <summary>
/// Coloque este componente na BattleScene.
/// Aplica o fundo salvo no GameManager no SpriteRenderer de fundo da batalha.
/// </summary>
public class BattleBackgroundConfig : MonoBehaviour
{
    [Tooltip("Nome do GameObject que contém o SpriteRenderer de fundo na BattleScene.")]
    public string nomeDoObjetoDeFundo = "BattleBackground";

    [Tooltip("Fundo padrão caso nenhum tenha sido definido.")]
    public Sprite fundoPadrao;

    private void Start()
    {
        GameObject obj = GameObject.Find(nomeDoObjetoDeFundo);
        if (obj == null)
        {
            Debug.LogWarning($"[BattleBackgroundConfig] Objeto '{nomeDoObjetoDeFundo}' não encontrado na BattleScene.");
            return;
        }

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"[BattleBackgroundConfig] Objeto '{nomeDoObjetoDeFundo}' não tem componente SpriteRenderer.");
            return;
        }

        Sprite sprite = GameManager.Instance?.battleBackground ?? fundoPadrao;

        if (sprite != null)
        {
            sr.sprite = sprite;
            Debug.Log($"[BattleBackgroundConfig] Fundo aplicado: '{sprite.name}'");
        }
        else
        {
            Debug.LogWarning("[BattleBackgroundConfig] Nenhum sprite de fundo definido.");
        }
    }
}