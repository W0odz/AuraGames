using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    [Header("Diálogo comum")]
    public DialogueAsset dialogoPadrao;

    [Header("Diálogo único (primeira vez)")]
    public DialogueAsset dialogoUnico;

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
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();

            if (!jaInteragiu)
            {
                if (dialogoUnico == null) dialogoUnico = dialogoPadrao;
                // Exibe diálogo único
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    if (merchant != null)
                        merchant.OpenMerchantMenu();
                });
                jaInteragiu = true;
            }
            else
            {  // Inicia o diálogo e só abre o menu quando ele terminar
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
}