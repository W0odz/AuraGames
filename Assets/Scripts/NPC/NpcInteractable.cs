using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    [Header("Diálogo comum")]
    public DialogueAsset dialogoPadrao;

    [Header("Diálogo único (primeira vez)")]
    public DialogueAsset dialogoUnico;

    [Header("Diálogos de Quest (opcional — deixe questVinculada vazio para ignorar)")]
    public QuestDefinition questVinculada;
    public DialogueAsset dialogoQuestIniciar;   // Exibido quando quest está NotStarted
    public DialogueAsset dialogoQuestAtivo;     // Exibido quando quest está Active
    public DialogueAsset dialogoQuestEntrega;   // Exibido quando quest está Completed

    private bool jaInteragiu = false;
    public bool isMerchant = false;
    private bool playerNearby = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }

    void Update()
    {
        if (GameManager.Instance.inputBloqueado) return;
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !DialogueRunner.Instance.IsDialogueActive)
            OnInteract();
    }

    public void OnInteract()
    {
        // Notifica o QuestManager sobre a conversa com esse NPC
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotificarConversa(gameObject.name);

        // Se há uma quest vinculada, seleciona o diálogo pelo estado dela
        if (questVinculada != null && QuestManager.Instance != null)
        {
            DialogueAsset dialogoQuest = SelecionarDialogoPorEstadoDeQuest();
            if (dialogoQuest != null)
            {
                if (isMerchant)
                {
                    NpcMerchant merchant = GetComponent<NpcMerchant>();
                    DialogueRunner.Instance.StartDialogue(dialogoQuest, () =>
                    {
                        if (merchant != null) merchant.OpenMerchantMenu();
                    });
                }
                else
                {
                    DialogueRunner.Instance.StartDialogue(dialogoQuest);
                }
                return;
            }
            // Se o diálogo do estado não foi preenchido, cai no comportamento padrão abaixo
        }

        // Comportamento original (sem quest vinculada ou diálogo do estado não preenchido)
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();

            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    if (merchant != null)
                        merchant.OpenMerchantMenu();
                });
                jaInteragiu = true;
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    if (merchant != null)
                        merchant.OpenMerchantMenu();
                });
            }
        }
        else
        {
            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                DialogueRunner.Instance.StartDialogue(dialogoUnico);
                jaInteragiu = true;
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao);
            }
        }
    }

    /// <summary>
    /// Retorna o DialogueAsset correto baseado no estado atual da questVinculada.
    /// Retorna null se o diálogo daquele estado não foi preenchido (cai no comportamento padrão).
    /// </summary>
    private DialogueAsset SelecionarDialogoPorEstadoDeQuest()
    {
        string id = questVinculada.questId;

        if (QuestManager.Instance.IsTurnedIn(id))
            return dialogoPadrao; // Quest já entregue → diálogo padrão

        if (QuestManager.Instance.IsCompleted(id))
            return dialogoQuestEntrega; // Completed → pronto para entregar

        if (QuestManager.Instance.IsActive(id))
            return dialogoQuestAtivo; // Active → em progresso

        // NotStarted
        return dialogoQuestIniciar;
    }
}