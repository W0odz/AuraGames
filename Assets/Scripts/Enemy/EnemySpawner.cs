using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuração de Spawn")]
    public GameObject enemyPrefab;
    public int numberOfEnemies = 5;

    [Header("Limites")]
    public Collider2D mapBoundsCollider;

    [Header("Área Segura do Jogador")]
    public float playerSafeZoneRadius = 5f; // Raio de segurança
    private Transform playerTransform; // Referência interna ao jogador

    #region Métodos Unity
    void Awake()
    {
        // Encontra o jogador na cena assim que ela carrega
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("O Spawner não conseguiu encontrar o Jogador na cena!");
            return;
        }

        if (mapBoundsCollider == null)
        {
            Debug.LogError("O Spawner precisa de uma referência ao Collider do Mapa (mapBoundsCollider)!");
            return;
        }

        SpawnEnemies();
    }
    #endregion

    void SpawnEnemies()
    {
        // Pega os limites retangulares *externos* do nosso colisor de mapa
        Bounds bounds = mapBoundsCollider.bounds;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector2 spawnPosition;

            // Tenta encontrar uma posição válida
            // (Isso impede que um inimigo nasça em um "buraco" do mapa)
            int attempts = 0;
            do
            {
                // Pega uma posição aleatória dentro do retângulo externo
                float spawnX = Random.Range(bounds.min.x, bounds.max.x);
                float spawnY = Random.Range(bounds.min.y, bounds.max.y);
                spawnPosition = new Vector2(spawnX, spawnY);

                attempts++;
                if (attempts > 50) // Medida de segurança
                {
                    Debug.LogWarning("Não foi possível encontrar um local de spawn válido após 50 tentativas.");
                    break; // Para o loop 'do-while'
                }

            } // Continua tentando enquanto o ponto aleatório NÃO estiver DENTRO do nosso colisor
            while (!mapBoundsCollider.OverlapPoint(spawnPosition));

            if (attempts > 50) continue; // Pula para o próximo inimigo no 'for'

            // Cria o inimigo
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            EnemyAIController ai = enemyGO.GetComponent<EnemyAIController>();
            if (ai != null)
            {
                // 1. Define o ID único deste inimigo
                string id = SceneManager.GetActiveScene().name + "_enemy_" + i;
                ai.enemyID = id;

                // 2. Define os limites do mapa
                ai.mapBoundsCollider = mapBoundsCollider;

                // 3. VERIFICA SE JÁ FOI DERROTADO
                if (GameManager.instance.defeatedEnemyIDs.Contains(id))
                {
                    // Se sim, chama a função de fade-out e desativação
                    ai.DefeatOnLoad();
                }
            }
        }
    }

    // Desenha o retângulo EXTERNO do nosso colisor para referência
    private void OnDrawGizmos()
    {
        if (mapBoundsCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(mapBoundsCollider.bounds.center, mapBoundsCollider.bounds.size);
        }
        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, playerSafeZoneRadius);
        }
        else if (Application.isPlaying == false)
        {
            // Tenta desenhar no editor mesmo sem estar jogando
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(player.transform.position, playerSafeZoneRadius);
            }
        }

    }
}