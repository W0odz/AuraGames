using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloque este componente na BattleScene.
/// Aplica o fundo salvo no GameManager na Image de fundo da batalha.
/// </summary>
public class BattleBackgroundConfig : MonoBehaviour
{
    [Tooltip("Nome do GameObject que contém a Image de fundo na BattleScene.")]
    public string nomeDoObjetoDeFundo = "BattleBackground";

    [Tooltip("Fundo padrão caso nenhum tenha sido definido.")]
    public Sprite fundoPadrao;

    private void Start()
    {
        // Busca a Image na BattleScene pelo nome do objeto
        GameObject obj = GameObject.Find(nomeDoObjetoDeFundo);
        if (obj == null)
        {
            Debug.LogWarning($"[BattleBackgroundConfig] Objeto '{nomeDoObjetoDeFundo}' não encontrado na BattleScene.");
            return;
        }

        Image img = obj.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning($"[BattleBackgroundConfig] Objeto '{nomeDoObjetoDeFundo}' não tem componente Image.");
            return;
        }

        Sprite sprite = GameManager.Instance?.battleBackground ?? fundoPadrao;

        if (sprite != null)
        {
            img.sprite = sprite;
            img.gameObject.SetActive(true);
            Debug.Log($"[BattleBackgroundConfig] Fundo aplicado: '{sprite.name}'");
        }
    }
}