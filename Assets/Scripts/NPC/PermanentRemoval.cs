using UnityEngine;

/// <summary>
/// Coloque este componente em qualquer NPC ou personagem que deve sumir permanentemente
/// da cena em algum momento do jogo. Configure um ID único no Inspector e, quando quiser
/// que ele suma para sempre, marque o checkbox "removido" e salve a cena.
/// A partir daí, mesmo saindo e voltando, o personagem não aparece mais.
/// </summary>
public class PermanentRemoval : MonoBehaviour
{
    [Tooltip("ID único deste personagem. Deve ser único entre todos os PermanentRemoval da cena.")]
    public string characterId;

    [Tooltip("Marque quando quiser que este personagem suma permanentemente. Salve a cena após marcar.")]
    public bool removido = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning($"[PermanentRemoval] '{gameObject.name}' não tem um characterId configurado.", this);
            return;
        }
        if (GameManager.Instance == null) return;

        // Se foi marcado como removido neste objeto, registra no GameManager
        if (removido)
            GameManager.Instance.MarcarPersonagemRemovido(characterId);

        // Desativa se já estava registrado como removido (inclusive em sessões anteriores)
        if (GameManager.Instance.PersonagemRemovido(characterId))
            gameObject.SetActive(false);
    }
}
