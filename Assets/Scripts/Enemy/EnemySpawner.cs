using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemies = 5;

    // ANTES: public Rect spawnBounds;
    // AGORA: Usamos o colisor do mapa como nosso limite
    public Collider2D mapBoundsCollider;

    #region Métodos Unity
    void Start()
    {
        // Se o colisor não foi definido, não podemos continuar
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
    }
}