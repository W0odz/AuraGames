using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueAsset dialogoPadrao;

    [Header("Diálogo único (primeira vez) — ignorado se questVinculada estiver preenchida")]
    public DialogueAsset dialogoUnico;

    [Header("Quest vinculada (opcional)")]
    [Tooltip("Se preenchida, os nós do dialogoPadrao serão filtrados pelo estado da quest. Deixe vazio para comportamento padrão.")]
    public QuestDefinition questVinculada;

    private bool jaInteragiu = false;
    public bool isMerchant = false;
    private bool playerNearby = false;

    // Tempo unscaled da última interação — evita reabrir o diálogo no mesmo frame que fechou
    private float ultimaInteracaoTime = -999f;
    private const float cooldownInteracao = 0.5f;

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
        if (!playerNearby) return;
        if (DialogueRunner.Instance.IsDialogueActive) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        // Cooldown: ignora se acabou de fechar o diálogo neste mesmo frame / instante
        if (Time.unscaledTime - ultimaInteracaoTime < cooldownInteracao) return;

        // Também ignora se o DialogueRunner acabou de fechar o diálogo
        if (Time.unscaledTime - DialogueRunner.Instance.ultimoFechamentoTime < cooldownInteracao) return;

        ultimaInteracaoTime = Time.unscaledTime;
        OnInteract();
    }

    public void OnInteract()
    {
        // Quest vinculada — com filtro de estado
        if (questVinculada != null)
        {
            if (dialogoPadrao == null)
            {
                Debug.LogWarning($"[NpcInteractable] {gameObject.name} tem questVinculada mas dialogoPadrao está vazio.");
                return;
            }

            if (isMerchant)
            {
                NpcMerchant merchant = GetComponent<NpcMerchant>();
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, questVinculada, () =>
                {
                    NotificarQuestManager();
                    if (merchant != null) merchant.OpenMerchantMenu();
                });
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, questVinculada, () =>
                {
                    NotificarQuestManager();
                });
            }
            return;
        }

        // Sem quest vinculada
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();
            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    NotificarQuestManager();
                    if (merchant != null) merchant.OpenMerchantMenu();
                });
                jaInteragiu = true;
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    NotificarQuestManager();
                    if (merchant != null) merchant.OpenMerchantMenu();
                });
            }
        }
        else
        {
            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    NotificarQuestManager();
                });
                jaInteragiu = true;
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    NotificarQuestManager();
                });
            }
        }
    }

    // Notifica ao final do diálogo, não no início
    private void NotificarQuestManager()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotificarConversa(gameObject);
    }
}