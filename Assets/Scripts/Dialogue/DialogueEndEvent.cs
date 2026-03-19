using UnityEngine;

public class DialogueEndEvent : MonoBehaviour
{
    [Header("Desativar ao fim do diálogo (temporário — volta ao reentrar na cena)")]
    [Tooltip("GameObjects que serão desativados quando o diálogo terminar. Voltam ao reentrar na cena.")]
    public GameObject[] desativarTemporariamente;

    [Header("Remover permanentemente ao fim do diálogo (Destroy — qualquer GameObject)")]
    [Tooltip("GameObjects que serão destruídos permanentemente desta cena ao fim do diálogo (não voltam).")]
    public GameObject[] removerObjetos;

    [Header("Remover NPC permanentemente (via PermanentRemoval — persiste entre sessões)")]
    [Tooltip("NPCs com PermanentRemoval que devem sumir para sempre após este diálogo.")]
    public PermanentRemoval[] removerPermanentemente;

    [Header("Transporte de Cena (opcional)")]
    [Tooltip("Nome da cena para onde o jogador será transportado ao fim do diálogo. Deixe vazio para não transportar.")]
    public string cenaDestino;

    [Tooltip("ID do SpawnPoint na cena destino onde o jogador vai aparecer. Deixe vazio para usar o spawn padrão.")]
    public string spawnIdDestino;

    public void ExecutarAcoes()
    {
        // Desativa temporariamente
        foreach (var obj in desativarTemporariamente)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Remove (Destroy) objetos permanentemente desta cena
        foreach (var obj in removerObjetos)
        {
            if (obj == null) continue;
            Debug.Log($"[DialogueEndEvent] '{obj.name}' destruído permanentemente da cena.");
            Destroy(obj);
        }

        // Remove NPCs permanentemente (via PermanentRemoval — persiste entre sessões)
        foreach (var pr in removerPermanentemente)
        {
            if (pr == null) continue;
            if (string.IsNullOrEmpty(pr.characterId))
            {
                Debug.LogWarning($"[DialogueEndEvent] PermanentRemoval em '{pr.gameObject.name}' não tem characterId configurado.");
                continue;
            }
            GameManager.Instance.MarcarPersonagemRemovido(pr.characterId);
            pr.gameObject.SetActive(false);
            Debug.Log($"[DialogueEndEvent] '{pr.gameObject.name}' removido permanentemente.");
        }

        // Transporta para outra cena
        if (!string.IsNullOrEmpty(cenaDestino))
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[DialogueEndEvent] GameManager.Instance é null — não foi possível transportar para a cena.");
                return;
            }

            GameManager.Instance.pendingSpawnID = !string.IsNullOrEmpty(spawnIdDestino)
                ? spawnIdDestino
                : null;

            Debug.Log($"[DialogueEndEvent] Transportando para cena '{cenaDestino}', spawn '{spawnIdDestino}'.");
            GameManager.Instance.LoadSceneWithFade(cenaDestino);
        }
    }
}