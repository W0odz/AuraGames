using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public bool isPassive = false; // Se marcado, ele só passeia

    [Header("Velocidades")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Identidade de Batalha")]
    public GameObject battlePrefab; // Qual inimigo eu sou na batalha?
    public bool isBoss = false;

    [Header("Configuracao de Perseguicao")]
    public float chaseDuration = 10f;
    private Coroutine chaseCoroutine;

    [Header("Referencias")]
    public Collider2D mapBoundsCollider;

    [Header("ID do inimigo")]
    public string enemyID; // Será definido pelo Spawner

    // --- VARIÁVEIS INTERNAS ---
    private Rigidbody2D rb;
    private Transform playerToChase;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private Vector2 wanderTarget;
    private Bounds bounds;
    private float currentMoveSpeed;

    private enum State
    {
        Wandering,
        Chasing
    }
    private State currentState;

    #region Métodos Unity
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Atribuição movida para cá
    }

    void Start()
    {
        currentState = State.Wandering;
        currentMoveSpeed = wanderSpeed;

        if (mapBoundsCollider != null)
        {
            bounds = mapBoundsCollider.bounds;
            PickNewWanderTarget();
        }
    }

    void Update()
    {
        if (mapBoundsCollider == null) return;

        switch (currentState)
        {
            case State.Wandering:
                if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
                {
                    PickNewWanderTarget();
                }
                moveDirection = (wanderTarget - (Vector2)transform.position).normalized;
                break;

            case State.Chasing:
                if (playerToChase != null)
                {
                    moveDirection = (playerToChase.position - transform.position).normalized;
                }
                else
                {
                    StopChasing();
                }
                break;
        }
    }

    // Logica de movimento (Fìsica)
    void FixedUpdate()
    {
        // ANTES: rb.MovePosition(rb.position + moveDirection * currentMoveSpeed * Time.fixedDeltaTime);
        // DEPOIS: Definimos a velocidade, e deixamos a física cuidar do resto
        rb.linearVelocity = moveDirection * currentMoveSpeed;
    }
    #endregion

    #region Sistema de Perseguição

    // Chamada pelo DetectionArea para INICIAR a perseguição
    public void StartChasing(Transform player)
    {

        if (isPassive) return; // Se for passivo, ignora o jogador

        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
        }

        playerToChase = player;
        currentState = State.Chasing;

        currentMoveSpeed = chaseSpeed;

        chaseCoroutine = StartCoroutine(ChaseTimerCoroutine());
    }

    // Chamada pelo timer (ou se o jogador sumir) para PARAR a perseguição
    public void StopChasing()
    {
        if (currentState == State.Chasing)
        {
            playerToChase = null;
            currentState = State.Wandering;

            currentMoveSpeed = wanderSpeed;

            PickNewWanderTarget();
            chaseCoroutine = null;
        }
    }

    // O timer de 10 segundos
    private IEnumerator ChaseTimerCoroutine()
    {
        yield return new WaitForSeconds(chaseDuration);
        StopChasing();
    }

    // Lógica para encontrar um ponto de passeio VÁLIDO dentro do mapa
    void PickNewWanderTarget()
    {
        int attempts = 0;
        do
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            wanderTarget = new Vector2(randomX, randomY);

            attempts++;
            if (attempts > 50)
            {
                wanderTarget = transform.position;
                break;
            }
        }
        while (!mapBoundsCollider.OverlapPoint(wanderTarget));
    }
    #endregion

    #region Desaparecimento pós derrota

    public void DefeatOnLoad()
    {
        // Começa a corrotina de fade e desativa
        gameObject.SetActive(false);
    }

    #endregion

    #region Congelamento dos inimigos
    public static void FreezeAllEnemies()
    {
        // Encontra todos os scripts de IA ativos na cena
        EnemyAIController[] allEnemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            // 1. Desativa o script (para parar de calcular perseguição/passeio)
            enemy.enabled = false;

            // 2. Para a física imediatamente (para não deslizar)
            if (enemy.rb != null)
            {
                enemy.rb.linearVelocity = Vector2.zero;
                enemy.rb.bodyType = RigidbodyType2D.Kinematic; // Trava no lugar
            }

            // 3. Para a animação (opcional, se tiver Animator)
            // Animator anim = enemy.GetComponent<Animator>();
            // if(anim) anim.enabled = false;
        }
    }
    #endregion
}