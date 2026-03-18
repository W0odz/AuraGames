using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trigger de diálogo no mapa. Ao entrar na área e pressionar E,
/// inicia um DialogueAsset — opcionalmente exibindo uma imagem de fundo fullscreen.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueAsset dialogo;

    [Tooltip("Se true, só dispara o diálogo uma vez")]
    public bool apenasUmaVez = false;

    [Header("Imagem de Fundo (opcional)")]
    [Tooltip("Imagem fullscreen exibida durante o diálogo (ex: interior de uma loja, outra área)")]
    public Sprite imagemFundo;

    [Tooltip("Referência ao Image do Canvas que será usado como fundo fullscreen")]
    public Image fundoUI;

    private bool playerDentro = false;
    private bool jaDisparou = false;

    private float ultimaInteracaoTime = -999f;
    private const float cooldownInteracao = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerDentro = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerDentro = false;
    }

    private void Update()
    {
        if (!playerDentro) return;
        if (apenasUmaVez && jaDisparou) return;
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;
        if (DialogueRunner.Instance.IsDialogueActive) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (Time.unscaledTime - ultimaInteracaoTime < cooldownInteracao) return;
        if (Time.unscaledTime - DialogueRunner.Instance.ultimoFechamentoTime < cooldownInteracao) return;

        ultimaInteracaoTime = Time.unscaledTime;
        Disparar();
    }

    private void Disparar()
    {
        if (dialogo == null)
        {
            Debug.LogWarning("[DialogueTrigger] Nenhum DialogueAsset atribuído.");
            return;
        }

        jaDisparou = true;

        // Ativa o fundo antes do diálogo
        if (fundoUI != null && imagemFundo != null)
        {
            fundoUI.sprite = imagemFundo;
            fundoUI.gameObject.SetActive(true);
        }

        // Ao terminar o diálogo, esconde o fundo
        DialogueRunner.Instance.StartDialogue(dialogo, () =>
        {
            if (fundoUI != null)
                fundoUI.gameObject.SetActive(false);

            // Permite disparar de novo se não for apenasUmaVez
            if (!apenasUmaVez)
                jaDisparou = false;
        });
    }
}