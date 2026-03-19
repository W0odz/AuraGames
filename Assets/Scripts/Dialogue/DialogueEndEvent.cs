using UnityEngine;

/// <summary>
/// Adicione este componente no mesmo GameObject de um DialogueTrigger ou NpcInteractable.
/// Ao fim do diálogo, desativa GameObjects temporários (voltam ao reentrar na cena)
/// e marca personagens como removidos permanentemente via PermanentRemoval.
/// ExecutarAcoes() deve ser chamado explicitamente pelo DialogueTrigger ou NpcInteractable
/// ao fim do diálogo correto, evitando que qualquer término de diálogo na cena dispare este evento.
/// </summary>
public class DialogueEndEvent : MonoBehaviour
{
    [Header("Desativar ao fim do diálogo (temporário — volta ao reentrar na cena)")]
    [Tooltip("GameObjects que serão desativados quando o diálogo terminar. Voltam ao reentrar na cena.")]
    public GameObject[] desativarTemporariamente;

    [Header("Remover permanentemente ao fim do diálogo")]
    [Tooltip("NPCs com PermanentRemoval que devem sumir para sempre após este diálogo.")]
    public PermanentRemoval[] removerPermanentemente;

    public void ExecutarAcoes()
    {
        // Desativa temporariamente
        foreach (var obj in desativarTemporariamente)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Remove permanentemente
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
    }
}
