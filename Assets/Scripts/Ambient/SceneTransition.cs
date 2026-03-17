using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string goToScene;

    [Tooltip("ID do ponto de spawn na cena de destino onde o jogador vai aparecer")]
    [SerializeField] private string spawnID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.pendingSpawnID = spawnID;
            GameManager.Instance.LoadSceneWithFade(goToScene);
        }
        else
            Debug.LogError("[SceneTransition] GameManager.Instance é null.");
    }
}
