// EnemyAIController.cs (COMPLETO E ATUALIZADO PARA F�SICA)
using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    [Header("Velocidades")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Configura��o de Persegui��o")]
    public float chaseDuration = 10f;
    private Coroutine chaseCoroutine;

    [Header("Refer�ncias")]
    public Collider2D mapBoundsCollider;

    // --- VARI�VEIS INTERNAS ---
    private Rigidbody2D rb;
    private Transform playerToChase;
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

    // L�gica de movimento (F�sica)
    void FixedUpdate()
    {
        // ANTES: rb.MovePosition(rb.position + moveDirection * currentMoveSpeed * Time.fixedDeltaTime);
        // DEPOIS: Definimos a velocidade, e deixamos a f�sica cuidar do resto
        rb.linearVelocity = moveDirection * currentMoveSpeed;
    }

    // Chamada pelo DetectionArea para INICIAR a persegui��o
    public void StartChasing(Transform player)
    {
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
        }

        playerToChase = player;
        currentState = State.Chasing;

        currentMoveSpeed = chaseSpeed;

        chaseCoroutine = StartCoroutine(ChaseTimerCoroutine());
    }

    // Chamada pelo timer (ou se o jogador sumir) para PARAR a persegui��o
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

    // L�gica para encontrar um ponto de passeio V�LIDO dentro do mapa
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
}