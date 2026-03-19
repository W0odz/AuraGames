using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string goToScene;

    [Tooltip("ID do ponto de spawn na cena de destino onde o jogador vai aparecer")]
    [SerializeField] private string spawnID;

    [Tooltip("Se true, a transição só ocorre ao pressionar E dentro do trigger")]
    [SerializeField] private bool precisaInteragir = false;

    [Header("Requisito de Item (opcional)")]
    [Tooltip("Nome do item necessário para passar. Deixe vazio para não exigir nenhum item.")]
    [SerializeField] private string itemNecessario = "";

    [Header("Transição Única (opcional)")]
    [Tooltip("Se true, essa transição só ocorre uma vez. Usa o transitionID para persistir.")]
    [SerializeField] private bool apenasUmaVez = false;

    [Tooltip("ID único desta transição. Deve ser único no jogo inteiro.")]
    [SerializeField] private string transitionID = "";

    private bool playerDentro = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!precisaInteragir)
        {
            Transicionar();
        }
        else
        {
            playerDentro = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerDentro = false;
    }

    private void Update()
    {
        if (!precisaInteragir) return;
        if (!playerDentro) return;
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;

        if (Input.GetKeyDown(KeyCode.E))
            Transicionar();
    }

    private void Transicionar()
    {
        // Verifica se já foi usada
        if (apenasUmaVez && !string.IsNullOrEmpty(transitionID))
        {
            if (GameManager.Instance != null && GameManager.Instance.TransicaoUsada(transitionID))
            {
                Debug.Log($"[SceneTransition] Transição '{transitionID}' já foi usada.");
                return;
            }
        }

        // Verifica se precisa de item
        if (!string.IsNullOrEmpty(itemNecessario))
        {
            if (InventoryManager.Instance == null || !InventoryManager.Instance.TemItem(itemNecessario))
            {
                Debug.Log($"[SceneTransition] Item necessário não encontrado: {itemNecessario}");
                return;
            }
        }

        // Marca como usada antes de transicionar
        if (apenasUmaVez && !string.IsNullOrEmpty(transitionID))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.MarcarTransicaoUsada(transitionID);
        }

        if (GameManager.Instance != null)
        {
            // Notifica o QuestManager sobre o uso da transição
            if (!string.IsNullOrEmpty(transitionID))
                QuestManager.Instance?.NotificarSceneTransition(transitionID);

            GameManager.Instance.pendingSpawnID = spawnID;
            GameManager.Instance.LoadSceneWithFade(goToScene);
        }
    }
}