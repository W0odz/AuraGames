using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    [Header("Configuração do Item")]
    public string itemID; // Ex: "Gema_Templo", "Chave_Porão"
    public string itemName; // Ex: "Gema Mística"

    void Start()
    {
        // Persistência: Se já pegamos este item antes, ele não deve aparecer
        if (GameManager.instance.collectedItemIDs.Contains(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Adiciona ao inventário no GameManager
            GameManager.instance.collectedItemIDs.Add(itemID);

            // 2. Feedback (Opcional: Som ou Texto)
            Debug.Log($"Você pegou: {itemName}");

            // 3. Some do mapa
            Destroy(gameObject);
        }
    }
}