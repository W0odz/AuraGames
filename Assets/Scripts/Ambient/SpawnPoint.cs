using UnityEngine;

/// <summary>
/// Marca um ponto de chegada na cena. O jogador é reposicionado aqui
/// quando a SceneTransition de origem tiver o mesmo spawnID.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("ID único deste ponto. Deve bater com o spawnID da SceneTransition de origem.")]
    public string spawnID;

    private void Start()
    {
        if (GameManager.Instance == null) return;
        if (string.IsNullOrEmpty(GameManager.Instance.pendingSpawnID)) return;

        if (GameManager.Instance.pendingSpawnID == spawnID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = transform.position;

            // Limpa o ID para não afetar próximas transições
            GameManager.Instance.pendingSpawnID = "";
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, spawnID);
    }
#endif
}
