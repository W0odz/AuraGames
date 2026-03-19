using UnityEngine;

/// <summary>
/// Adicione este componente no mesmo GameObject de um DialogueTrigger ou NpcInteractable.
/// Ao fim do diálogo, desativa GameObjects temporários (voltam ao reentrar na cena)
/// e marca personagens como removidos permanentemente via PermanentRemoval.
/// </summary>
public class DialogueEndEvent : MonoBehaviour
{
    [Header("Desativar ao fim do diálogo (temporário — volta ao reentrar na cena)")]
    [Tooltip("GameObjects que serão desativados quando o diálogo terminar. Voltam ao reentrar na cena.")]
    public GameObject[] desativarTemporariamente;

    [Header("Remover permanentemente ao fim do diálogo")]
    [Tooltip("NPCs com PermanentRemoval que devem sumir para sempre após este diálogo.")]
    public PermanentRemoval[] removerPermanentemente;

    private bool _registrado = false;

    private void OnEnable()
    {
        if (DialogueRunner.Instance != null && !_registrado)
        {
            DialogueRunner.Instance.onDialogueEnd += OnDialogueEnd;
            _registrado = true;
        }
    }

    private void Start()
    {
        // Fallback: registra caso DialogueRunner ainda não existia durante OnEnable
        if (DialogueRunner.Instance != null && !_registrado)
        {
            DialogueRunner.Instance.onDialogueEnd += OnDialogueEnd;
            _registrado = true;
        }
    }

    private void OnDisable()
    {
        if (DialogueRunner.Instance != null)
        {
            DialogueRunner.Instance.onDialogueEnd -= OnDialogueEnd;
            _registrado = false;
        }
    }

    private void OnDialogueEnd()
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

        // Se desregistra após disparar para não ser acionado por outros diálogos futuros na cena
        DialogueRunner.Instance.onDialogueEnd -= OnDialogueEnd;
        _registrado = false;
    }
}
