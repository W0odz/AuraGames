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
    [Tooltip("Raio mínimo para NÃO spawnar inimigos perto do player (normal).")]
    public float playerSafeZoneRadius = 5f;

    [Tooltip("Raio usado logo após voltar de batalha (durante a grace period).")]
    public float playerSafeZoneRadiusDuringGrace = 9f;

    [Tooltip("Quantas tentativas de achar um ponto válido de spawn antes de desistir.")]
    public int maxSpawnAttempts = 50;

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
        StartCoroutine(StartSpawnerNextFrame());
    }

    IEnumerator StartSpawnerNextFrame()
    {
        yield return null; // espera 1 frame pro PlayerMovement.Start() reposicionar
        StartCoroutine(DistanceCheckRoutine());
    }

    float GetCurrentSafeRadius()
    {
        // IMPORTANTE:
        // Esse método depende de você ter implementado no GameManager:
        // bool IsInCombatGracePeriod()
        // (ou troque para IsInReturnGracePeriod, conforme você decidir padronizar)
        if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod())
            return Mathf.Max(playerSafeZoneRadius, playerSafeZoneRadiusDuringGrace);

        return playerSafeZoneRadius;
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
                    if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod())
                    {
                        // mantém ativo até acabar o grace
                    }
                    else
                    {
                        DeactivateSpawner();
                    }
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

        // Durante o CombatGrace, NÃO destrua inimigos (evita sumiço ao voltar da batalha)
        if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod())
            return;

        // comportamento normal (performance)
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                Destroy(enemies[i]);
                enemies[i] = null;
            }
        }
    }

    IEnumerator SpawnLogicRoutine()
    {
        while (isActive)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod())
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            for (int i = 0; i < numberOfEnemies; i++)
            {
                if (enemies[i] == null && !isRespawning[i])
                {
                    CheckAndSpawnSlot(i);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void CheckAndSpawnSlot(int slotIndex)
    {
        string id = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_Enemy_" + slotIndex;

        if (GameManager.Instance.defeatedEnemyIDs.Contains(id))
        {
            StartCoroutine(RespawnTimer(slotIndex, id));
        }
        else
        {
            SpawnEnemyInSlot(slotIndex, id);
        }
    }

    bool TryFindValidSpawnPosition(out Vector2 spawnPosition)
    {
        spawnPosition = default;

        if (mapBoundsCollider == null)
            return false;

        Bounds bounds = mapBoundsCollider.bounds;
        float safeRadius = GetCurrentSafeRadius();

        for (int attempts = 0; attempts < maxSpawnAttempts; attempts++)
        {
            float spawnX = Random.Range(bounds.min.x, bounds.max.x);
            float spawnY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 candidate = new Vector2(spawnX, spawnY);

            bool isInsideMap = mapBoundsCollider.OverlapPoint(candidate);

            float distToPlayer = (playerTransform != null)
                ? Vector2.Distance(candidate, playerTransform.position)
                : 999f;

            bool isTooCloseToPlayer = distToPlayer < safeRadius;

            if (isInsideMap && !isTooCloseToPlayer)
            {
                spawnPosition = candidate;
                return true;
            }
        }

        return false;
    }

    void SpawnEnemyInSlot(int slotIndex, string id)
    {
        if (!TryFindValidSpawnPosition(out Vector2 spawnPosition))
            return;

        GameObject enemyGO = Instantiate(explorationPrefab, spawnPosition, Quaternion.identity);
        enemies[slotIndex] = enemyGO;

        if (battlePrefab != null) enemyGO.transform.localScale = battlePrefab.transform.localScale;

        SpriteRenderer expRend = enemyGO.GetComponent<SpriteRenderer>();
        if (expRend == null) expRend = enemyGO.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer batRend = battlePrefab.GetComponent<SpriteRenderer>();
        if (batRend == null) batRend = battlePrefab.GetComponentInChildren<SpriteRenderer>();

        if (expRend != null && batRend != null)
        {
            expRend.sprite = batRend.sprite;
            expRend.color = batRend.color;
        }

        EnemyAIController ai = enemyGO.GetComponent<EnemyAIController>();
        if (ai != null)
        {
            ai.battlePrefab = battlePrefab;
            ai.enemyID = id;
            ai.mapBoundsCollider = mapBoundsCollider;
        }
    }

    IEnumerator RespawnTimer(int slotIndex, string id)
    {
        isRespawning[slotIndex] = true;

        yield return new WaitForSeconds(respawnTime);

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

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, playerSafeZoneRadius);
        }
    }
}