using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string goToScene;

    [Tooltip("ID do ponto de spawn na cena de destino onde o jogador vai aparecer")]
    [SerializeField] private string spawnID;

    [Tooltip("Se true, a transição só ocorre ao pressionar E dentro do trigger")]
    [SerializeField] private bool precisaInteragir = false;

    private bool playerDentro = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!precisaInteragir)
        {
            // Comportamento original — transição automática
            Transicionar();
        }
        else
        {
            // Aguarda o jogador pressionar E
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.pendingSpawnID = spawnID;
            GameManager.Instance.LoadSceneWithFade(goToScene);
        }
        else
            Debug.LogError("[SceneTransition] GameManager.Instance é null.");
    }
}