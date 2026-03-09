using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    public DialogueAsset dialogueAsset;
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
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !DialogueRunner.Instance.IsDialogueActive)
            OnInteract();
    }

    public void OnInteract()
    {
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();
            // Inicia o diálogo e só abre o menu quando ele terminar
            DialogueRunner.Instance.StartDialogue(dialogueAsset, () =>
            {
                if (merchant != null)
                    merchant.OpenMerchantMenu();
            });
        }
        else
        {
            DialogueRunner.Instance.StartDialogue(dialogueAsset);
        }
    }
}