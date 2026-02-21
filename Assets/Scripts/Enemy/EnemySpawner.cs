using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Otimização / Performance")]
    public float activationDistance = 20f;
    public float deactivationBuffer = 5f;
    private bool isActive = false;
    private Coroutine spawnCoroutine;

    [Header("Configuração de Spawn")]
    public GameObject explorationPrefab;
    public GameObject battlePrefab;
    public int numberOfEnemies = 5;
    public float respawnTime = 10f;

    [Header("Limites")]
    public Collider2D mapBoundsCollider;

    [Header("Área Segura do Jogador")]
    public float playerSafeZoneRadius = 5f;

    private Transform playerTransform;

    // MUDANÇA: Em vez de uma Lista dinâmica, usamos um Array fixo (Slots)
    // Se enemies[0] for null, o slot 0 está vazio.
    private GameObject[] enemies;

    // Controla quais slots estão esperando para renascer para não duplicar coroutines
    private bool[] isRespawning;

    void Awake()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTransform = player.transform;
        else Debug.LogError("Spawner não achou o Jogador!");

        if (mapBoundsCollider == null) Debug.LogError("Spawner sem Collider de Mapa!");

        // Inicializa os arrays de slots
        enemies = new GameObject[numberOfEnemies];
        isRespawning = new bool[numberOfEnemies];
    }

    void Start()
    {
        StartCoroutine(DistanceCheckRoutine());
    }

    IEnumerator DistanceCheckRoutine()
    {
        WaitForSeconds checkInterval = new WaitForSeconds(0.5f);

        while (true)
        {
            if (playerTransform != null && mapBoundsCollider != null)
            {
                Vector2 closestPointOnZone = mapBoundsCollider.ClosestPoint(playerTransform.position);
                float distance = Vector2.Distance(closestPointOnZone, playerTransform.position);
                float disableDistance = activationDistance + deactivationBuffer;

                if (!isActive && distance <= activationDistance)
                {
                    ActivateSpawner();
                }
                else if (isActive && distance > disableDistance)
                {
                    DeactivateSpawner();
                }
            }
            yield return checkInterval;
        }
    }

    void ActivateSpawner()
    {
        isActive = true;
        spawnCoroutine = StartCoroutine(SpawnLogicRoutine());
    }

    void DeactivateSpawner()
    {
        isActive = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        // Destroi os inimigos visuais, mas mantemos a lógica dos slots
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                Destroy(enemies[i]);
                enemies[i] = null;
            }
        }
        // Nota: Não limpamos o 'isRespawning', pois o tempo continua correndo mesmo longe
    }

    IEnumerator SpawnLogicRoutine()
    {
        while (isActive)
        {
            // Itera por cada "Cadeira" (Slot) disponível
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Se a cadeira está vazia E não está no meio de um processo de respawn
                if (enemies[i] == null && !isRespawning[i])
                {
                    CheckAndSpawnSlot(i);
                }
            }

            // Verifica a cada segundo
            yield return new WaitForSeconds(1f);
        }
    }

    void CheckAndSpawnSlot(int slotIndex)
    {
        // Gera um ID FIXO baseado no nome da cena, nome do spawner e NÚMERO DO SLOT
        // Exemplo: "Floresta_SpawnZone_Goblin_Enemy_0"
        // Esse ID será sempre o mesmo para esse slot, permitindo persistência!
        string id = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_Enemy_" + slotIndex;

        // Verifica se este ID específico está na lista de mortos
        if (GameManager.Instance.defeatedEnemyIDs.Contains(id))
        {
            // Ele está morto! Começa o processo de Respawn
            StartCoroutine(RespawnTimer(slotIndex, id));
        }
        else
        {
            // Ele está vivo (não está na lista)! Pode spawnar.
            SpawnEnemyInSlot(slotIndex, id);
        }
    }

    void SpawnEnemyInSlot(int slotIndex, string id)
    {
        Bounds bounds = mapBoundsCollider.bounds;
        Vector2 spawnPosition;
        int attempts = 0;
        bool isInsideMap;
        bool isTooCloseToPlayer;

        do
        {
            float spawnX = Random.Range(bounds.min.x, bounds.max.x);
            float spawnY = Random.Range(bounds.min.y, bounds.max.y);
            spawnPosition = new Vector2(spawnX, spawnY);

            isInsideMap = mapBoundsCollider.OverlapPoint(spawnPosition);
            float distToPlayer = (playerTransform != null) ? Vector2.Distance(spawnPosition, playerTransform.position) : 999f;
            isTooCloseToPlayer = distToPlayer < playerSafeZoneRadius;

            attempts++;
            if (attempts > 50) return;

        } while (!isInsideMap || isTooCloseToPlayer);

        // Cria o inimigo
        GameObject enemyGO = Instantiate(explorationPrefab, spawnPosition, Quaternion.identity);

        // Coloca ele no Slot correto do Array
        enemies[slotIndex] = enemyGO;

        // --- CONFIGURAÇÃO VISUAL ---
        // Configura Escala
        if (battlePrefab != null) enemyGO.transform.localScale = battlePrefab.transform.localScale;

        // Configura Sprite e Cor
        SpriteRenderer expRend = enemyGO.GetComponent<SpriteRenderer>();
        if (expRend == null) expRend = enemyGO.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer batRend = battlePrefab.GetComponent<SpriteRenderer>();
        if (batRend == null) batRend = battlePrefab.GetComponentInChildren<SpriteRenderer>();

        if (expRend != null && batRend != null)
        {
            expRend.sprite = batRend.sprite;
            expRend.color = batRend.color;
        }

        // --- CONFIGURAÇÃO IA ---
        EnemyAIController ai = enemyGO.GetComponent<EnemyAIController>();
        if (ai != null)
        {
            ai.battlePrefab = battlePrefab;
            ai.enemyID = id; // Usa o ID fixo do slot
            ai.mapBoundsCollider = mapBoundsCollider;
        }
    }

    // Rotina que espera o tempo passar para "reviver" um inimigo morto
    IEnumerator RespawnTimer(int slotIndex, string id)
    {
        isRespawning[slotIndex] = true;

        // Espera o tempo configurado
        yield return new WaitForSeconds(respawnTime);

        // Remove o ID da lista de mortos do GameManager
        // Agora, na próxima checagem do loop principal, ele será considerado "vivo" e vai spawnar
        if (GameManager.Instance.defeatedEnemyIDs.Contains(id))
        {
            GameManager.Instance.defeatedEnemyIDs.Remove(id);
        }

        isRespawning[slotIndex] = false;
    }

    private void OnDrawGizmos()
    {
        if (mapBoundsCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(mapBoundsCollider.bounds.center, mapBoundsCollider.bounds.size);
        }

        if (playerTransform == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null && mapBoundsCollider != null)
        {
            Vector2 closestPoint = mapBoundsCollider.ClosestPoint(playerTransform.position);
            float distance = Vector2.Distance(closestPoint, playerTransform.position);

            if (distance <= activationDistance) Gizmos.color = Color.green;
            else if (distance <= activationDistance + deactivationBuffer) Gizmos.color = Color.yellow;
            else Gizmos.color = Color.gray;

            Gizmos.DrawLine(playerTransform.position, closestPoint);
            Gizmos.DrawWireSphere(closestPoint, 0.5f);
        }
    }
}