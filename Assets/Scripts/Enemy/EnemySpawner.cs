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
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Awake()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTransform = player.transform;
        else Debug.LogError("Spawner não achou o Jogador!");

        if (mapBoundsCollider == null) Debug.LogError("Spawner sem Collider de Mapa!");
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

        foreach (GameObject enemy in aliveEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        aliveEnemies.Clear();
    }

    IEnumerator SpawnLogicRoutine()
    {
        while (aliveEnemies.Count < numberOfEnemies)
        {
            TrySpawnOneEnemy();
            yield return null;
        }

        while (isActive)
        {
            yield return new WaitForSeconds(respawnTime);
            aliveEnemies.RemoveAll(item => item == null || !item.activeInHierarchy);

            if (aliveEnemies.Count < numberOfEnemies)
            {
                TrySpawnOneEnemy();
            }
        }
    }

    void TrySpawnOneEnemy()
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

        GameObject enemyGO = Instantiate(explorationPrefab, spawnPosition, Quaternion.identity);
        aliveEnemies.Add(enemyGO);

        // --- ATUALIZAÇÃO VISUAL (Sprite + Cor + Escala) ---

        // 1. Copia a Escala (NOVO!)
        // Aplica a escala do prefab de batalha no inimigo do mundo
        if (battlePrefab != null)
        {
            enemyGO.transform.localScale = battlePrefab.transform.localScale;
        }

        // 2. Copia Sprite e Cor
        SpriteRenderer expRend = enemyGO.GetComponent<SpriteRenderer>();
        if (expRend == null) expRend = enemyGO.GetComponentInChildren<SpriteRenderer>();

        SpriteRenderer batRend = battlePrefab.GetComponent<SpriteRenderer>();
        if (batRend == null) batRend = battlePrefab.GetComponentInChildren<SpriteRenderer>();

        if (expRend != null && batRend != null)
        {
            expRend.sprite = batRend.sprite;
            expRend.color = batRend.color;
        }
        // --------------------------------------------------

        EnemyAIController ai = enemyGO.GetComponent<EnemyAIController>();
        if (ai != null)
        {
            ai.battlePrefab = battlePrefab;
            string uniqueSuffix = System.DateTime.Now.Ticks.ToString() + "_" + Random.Range(0, 1000);
            string id = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_" + uniqueSuffix;
            ai.enemyID = id;
            ai.mapBoundsCollider = mapBoundsCollider;
        }
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