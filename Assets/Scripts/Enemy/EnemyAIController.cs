using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public bool isPassive = false; // Se marcado, ele só passeia

    [Header("Comportamento")]
    [Tooltip("Se true, o inimigo fica parado no lugar — não anda nem persegue. Ainda inicia batalha ao contato.")]
    public bool isEstatico = false;

    [Header("Velocidades")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Identidade de Batalha")]
    public GameObject battlePrefab;
    public bool isBoss = false;

    [Header("Diálogo Pré-Batalha (opcional)")]
    [Tooltip("Se preenchido, este diálogo toca antes de entrar na batalha.")]
    public DialogueAsset dialogoPreBatalha;

    [Header("Transição com Imagem (opcional)")]
    [Tooltip("Se preenchido, exibe esta imagem estática em tela cheia antes de entrar na batalha.")]
    public Sprite imagemTransicaoBatalha;

    [Tooltip("Segundos mínimos que a imagem fica visível antes do fade para a batalha. Mínimo garantido: 3s.")]
    [Min(3f)]
    public float duracaoImagemTransicao = 3f;

    [Header("Configuracao de Perseguicao")]
    public float chaseDuration = 10f;
    private Coroutine chaseCoroutine;

    [Header("Referencias")]
    public Collider2D mapBoundsCollider;

    [Header("Detecção / Aggro (opcional)")]
    [Tooltip("Triggers/Colliders2D usados para detectar o player.")]
    public Collider2D[] aggroDetectors;

    [Header("ID do inimigo")]
    public string enemyID;

    // --- VARIÁVEIS INTERNAS ---
    private Rigidbody2D rb;
    private Transform playerToChase;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private Vector2 wanderTarget;
    private Bounds bounds;
    private float currentMoveSpeed;

    private bool isAggroSuppressed;

    private enum State { Wandering, Chasing }
    private State currentState;

    #region Métodos Unity
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentState = State.Wandering;
        currentMoveSpeed = wanderSpeed;

        if (!isEstatico && mapBoundsCollider != null)
        {
            bounds = mapBoundsCollider.bounds;
            PickNewWanderTarget();
        }
    }

    void Update()
    {
        // Estático — não faz nada de movimento
        if (isEstatico) return;

        if (mapBoundsCollider == null) return;

        bool shouldSuppress = (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod());
        if (shouldSuppress != isAggroSuppressed)
            SetAggroSuppressed(shouldSuppress);

        if (isAggroSuppressed && currentState == State.Chasing)
            StopChasing();

        switch (currentState)
        {
            case State.Wandering:
                if (Vector2.Distance(transform.position, wanderTarget) < 0.5f)
                    PickNewWanderTarget();
                moveDirection = (wanderTarget - (Vector2)transform.position).normalized;
                break;

            case State.Chasing:
                if (playerToChase != null)
                    moveDirection = (playerToChase.position - transform.position).normalized;
                else
                    StopChasing();
                break;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Estático — garante que não se mova
        if (isEstatico)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * currentMoveSpeed;
    }
    #endregion

    #region Grace period / supressão de aggro
    void SetAggroSuppressed(bool suppressed)
    {
        isAggroSuppressed = suppressed;

        if (aggroDetectors != null)
            foreach (var col in aggroDetectors)
                if (col != null) col.enabled = !suppressed;

        if (suppressed)
            StopChasing();
    }
    #endregion

    #region Sistema de Perseguição
    public void StartChasing(Transform player)
    {
        if (isEstatico) return; // Estático nunca persegue
        if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod()) return;
        if (isAggroSuppressed) return;
        if (isPassive) return;

        if (chaseCoroutine != null)
            StopCoroutine(chaseCoroutine);

        playerToChase = player;
        currentState = State.Chasing;
        currentMoveSpeed = chaseSpeed;

        chaseCoroutine = StartCoroutine(ChaseTimerCoroutine());
    }

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

    private IEnumerator ChaseTimerCoroutine()
    {
        yield return new WaitForSeconds(chaseDuration);
        StopChasing();
    }

    void PickNewWanderTarget()
    {
        if (mapBoundsCollider == null) return;

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
        gameObject.SetActive(false);
    }
    #endregion

    #region Congelamento dos inimigos
    public static void FreezeAllEnemies()
    {
        EnemyAIController[] allEnemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            enemy.enabled = false;

            if (enemy.rb != null)
            {
                enemy.rb.linearVelocity = Vector2.zero;
                enemy.rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }
    #endregion
}