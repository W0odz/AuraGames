using UnityEngine;

public class BarreiraMagica : MonoBehaviour
{
    [Header("Requisitos")]
    public string requiredItemID; // O ID do item necessário (Ex: "Gema_Templo")

    [Header("Feedback")]
    public string lockedMessage = "Uma barreira mágica bloqueia o caminho.";
    public string unlockedMessage = "A gema brilhou e a barreira se dissipou!";

    // Usamos OnCollisionEnter porque a barreira deve ser SÓLIDA (empurrar o jogador)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckKey();
        }
    }

    void CheckKey()
    {
        // Verifica se o jogador tem o item no inventário do GameManager
        if (GameManager.instance.collectedItemIDs.Contains(requiredItemID))
        {
            // TEM A CHAVE: Abre a barreira
            Debug.Log(unlockedMessage);

            // Toca um som, efeito de partícula, etc.

            Destroy(gameObject); // Remove a barreira
        }
        else
        {
            // NÃO TEM A CHAVE
            Debug.Log(lockedMessage);
        }
    }
}