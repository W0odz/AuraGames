using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    public enum ModoDisparo { AoPressionarE, AoEntrarNaArea }

    [Header("Diálogo")]
    public DialogueAsset dialogo;

    [Tooltip("Define como o diálogo é disparado.")]
    public ModoDisparo modoDisparo = ModoDisparo.AoPressionarE;

    [Tooltip("Se true, só dispara o diálogo uma vez")]
    public bool apenasUmaVez = false;

    [Header("Quest (opcional)")]
    [Tooltip("ID único deste trigger. Deve bater com o 'triggerDialogueId' no QuestObjective.")]
    public string triggerId;

    [Header("Imagem de Fundo (opcional)")]
    public Sprite imagemFundo;
    public Image fundoUI;

    [Header("Alterar Velocidade do Jogador (opcional)")]
    [Tooltip("Se true, altera a velocidade do jogador ao fim do diálogo.")]
    public bool alterarVelocidade = false;

    [Tooltip("Nova velocidade do jogador após o diálogo.")]
    public float novaVelocidade = 6f;

    private bool playerDentro = false;
    private bool jaDisparou = false;

    private float ultimaInteracaoTime = -999f;
    private const float cooldownInteracao = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerDentro = true;

        if (modoDisparo == ModoDisparo.AoEntrarNaArea)
            TentarDisparar();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerDentro = false;
    }

    private void Update()
    {
        if (modoDisparo != ModoDisparo.AoPressionarE) return;

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

    private void TentarDisparar()
    {
        if (apenasUmaVez && jaDisparou) return;
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;
        if (DialogueRunner.Instance.IsDialogueActive) return;

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

        if (!string.IsNullOrEmpty(triggerId) && QuestManager.Instance != null)
            QuestManager.Instance.NotificarDialogueTrigger(triggerId);

        if (fundoUI != null && imagemFundo != null)
        {
            fundoUI.sprite = imagemFundo;
            fundoUI.gameObject.SetActive(true);
        }

        DialogueRunner.Instance.StartDialogue(dialogo, () =>
        {
            if (fundoUI != null)
                fundoUI.gameObject.SetActive(false);

            // Altera a velocidade do jogador ao fim do diálogo
            if (alterarVelocidade)
            {
                var pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null)
                {
                    pm.moveSpeed = novaVelocidade;
                    Debug.Log($"[DialogueTrigger] Velocidade do jogador alterada para {novaVelocidade}.");
                }
            }

            if (!apenasUmaVez)
                jaDisparou = false;
        });
    }
}