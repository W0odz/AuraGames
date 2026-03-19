using UnityEngine;

/// <summary>
/// Coloque este componente em qualquer GameObject da cena.
/// Permite remover personagens permanentemente ao:
///   1. O jogador coletar um item específico (itemGatilho).
///   2. O jogador vencer duas batalhas específicas (enemyIdBatalha1 e enemyIdBatalha2).
/// Cada grupo de remoção é independente — pode usar um, o outro ou ambos.
/// </summary>
public class RemovalTrigger : MonoBehaviour
{
    [Header("─── Gatilho: Coletar Item ───")]
    [Tooltip("Item que, ao ser coletado pelo jogador, dispara a remoção.")]
    public DadosItem itemGatilho;

    [Tooltip("Personagens a remover permanentemente ao coletar o item.")]
    public PermanentRemoval[] removerAoColetarItem;

    [Header("─── Gatilho: Duas Batalhas Específicas ───")]
    [Tooltip("ID do primeiro inimigo (currentEnemyID do GameManager) que deve ser derrotado.")]
    public string enemyIdBatalha1;

    [Tooltip("ID do segundo inimigo (currentEnemyID do GameManager) que deve ser derrotado.")]
    public string enemyIdBatalha2;

    [Tooltip("Personagens a remover permanentemente após ambas as batalhas.")]
    public PermanentRemoval[] removerAposBatalhas;

    // Estado interno
    private bool _itemGatilhoDisparado = false;
    private bool _batalhasDisparadas = false;

    private void Update()
    {
        // --- Gatilho de item ---
        if (!_itemGatilhoDisparado && itemGatilho != null && InventoryManager.Instance != null)
        {
            bool temItem = InventoryManager.Instance.listaItens.Exists(s => s.item == itemGatilho && s.quantidade > 0);
            if (temItem)
            {
                _itemGatilhoDisparado = true;
                ExecutarRemocao(removerAoColetarItem, "item");
            }
        }

        // --- Gatilho de duas batalhas ---
        if (!_batalhasDisparadas && GameManager.Instance != null)
        {
            bool primeiroDerrotado = !string.IsNullOrEmpty(enemyIdBatalha1)
                      && GameManager.Instance.defeatedEnemyIDs.Contains(enemyIdBatalha1);
            bool segundoDerrotado = !string.IsNullOrEmpty(enemyIdBatalha2)
                      && GameManager.Instance.defeatedEnemyIDs.Contains(enemyIdBatalha2);

            if (primeiroDerrotado && segundoDerrotado)
            {
                _batalhasDisparadas = true;
                ExecutarRemocao(removerAposBatalhas, "batalhas");
            }
        }
    }

    private void ExecutarRemocao(PermanentRemoval[] alvos, string origem)
    {
        if (alvos == null) return;

        foreach (var pr in alvos)
        {
            if (pr == null) continue;

            if (string.IsNullOrEmpty(pr.characterId))
            {
                Debug.LogWarning($"[RemovalTrigger] PermanentRemoval em '{pr.gameObject.name}' não tem characterId configurado.");
                continue;
            }

            if (GameManager.Instance == null) return;

            GameManager.Instance.MarcarPersonagemRemovido(pr.characterId);
            pr.gameObject.SetActive(false);
            Debug.Log($"[RemovalTrigger] '{pr.gameObject.name}' removido permanentemente (gatilho: {origem}).");
        }
    }
}
