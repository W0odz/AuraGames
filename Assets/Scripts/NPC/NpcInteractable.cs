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
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotificarConversa(gameObject.name);

        // Se há quest vinculada, usa sempre o dialogoPadrao com filtro de estado
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
                    if (merchant != null) merchant.OpenMerchantMenu();
                });
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, questVinculada);
            }
            return;
        }

        // Comportamento original — sem quest vinculada
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();
            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    if (merchant != null) merchant.OpenMerchantMenu();
                });
                jaInteragiu = true;
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    if (merchant != null) merchant.OpenMerchantMenu();
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
}