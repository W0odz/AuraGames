using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    public DialogueAsset dialogueAsset;
    public bool isMerchant = false; // Marque no Inspector se for um mercante!

    private bool playerNearby = false;

    // Dica: message pop-up ("Aperte E para interagir") pode ir aqui depois!
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    void Update()
    {
        // Só reage quando: player está perto, diálogo não está ativo e pressionou E
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !DialogueRunner.Instance.IsDialogueActive)
            OnInteract();
    }

    // Chame sempre por aqui, não acesse StartDialogue direto fora do NPC!
    public void OnInteract()
    {
        DialogueRunner.Instance.StartDialogue(dialogueAsset);

        if (isMerchant)
        {
            // TODO: Chamar MerchantMenu.Instance.Open(this); quando o menu de escambo estiver pronto.
        }
    }
}