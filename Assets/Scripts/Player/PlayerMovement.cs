using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para trocar de cena

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade do jogador

    private Rigidbody2D rb;
    private Vector2 moveInput;

    [SerializeField] private float enemySafeRadiusOnReturn = 2.5f;


    #region Métodos Unity
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Pega a referência do Rigidbody
    }

    void Start()
    {
        // Prioridade 1: Voltando de Batalha (Curto Prazo)
        if (GameManager.Instance.isReturningFromBattle)
        {
            transform.position = GameManager.Instance.playerReturnPosition;
            GameManager.Instance.isReturningFromBattle = false;
            RepelNearbyEnemies();
        }
        // Prioridade 2: Carregando um Save (Longo Prazo) --- NOVO ---
        else if (GameManager.Instance.isLoadingSave)
        {
            transform.position = GameManager.Instance.positionToLoad;

            // Desliga a flag para não teleportar de novo se trocar de sala
            GameManager.Instance.isLoadingSave = false;
        }
        // Se nenhuma das duas for verdade, o jogador nasce no local padrão da cena
    }

    void Update()
    {
        // Pega o input do teclado (Setas ou WASD)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // .normalized garante que o movimento na diagonal
        // nao seja mais rápido que o normal.
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    // FixedUpdate é chamado em um intervalo fixo (ideal para f�sica)
    void FixedUpdate()
    {
        // ANTES: rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        // DEPOIS: Definimos a velocidade, e deixamos a f�sica cuidar do resto
        rb.linearVelocity = moveInput * moveSpeed;
    }


    // Esta função é chamada automaticamente pela Unity
    // quando nosso colisor SÓLIDO (do jogador) entra em um colisor "Is Trigger"
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInCombatGracePeriod())
            return;

        // Verificamos se a coisa que tocamos tem a tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            // Pega a IA do inimigo para saber o ID dele
            EnemyAIController ai = other.GetComponent<EnemyAIController>();

            if (ai != null && ai.isPassive)
            {
                return; // Sai da função imediatamente
            }

            if (ai != null)
            {

                if (ai.isBoss)
                {
                    Debug.Log("PLAYER: Encontrei um CHEFE! Ativando modo Boss Battle.");
                    GameManager.Instance.isBossBattle = true;
                }
                else
                {
                    GameManager.Instance.isBossBattle = false;
                }

                //Congela tudo imediatamente
                EnemyAIController.FreezeAllEnemies();

                // Desativa o movimento do PRÓPRIO jogador também
                // para ele não continuar andando durante o fade
                this.enabled = false;
                rb.linearVelocity = Vector2.zero;

                // Salva qual inimigo estamos lutando
                GameManager.Instance.currentEnemyID = ai.enemyID;

                // Salva de qual cena estamos vindo
                GameManager.Instance.lastExplorationScene = SceneManager.GetActiveScene().name;

                // Antes de ir para a batalha, salva onde estamos
                GameManager.Instance.playerReturnPosition = transform.position;
                GameManager.Instance.isReturningFromBattle = true;

                // Pega o prefab de batalha do inimigo e manda pro GameManager
                if (ai.battlePrefab != null)
                {
                    GameManager.Instance.nextBattleEnemyPrefab = ai.battlePrefab;
                }
                else
                {
                    Debug.LogError("O inimigo de exploração não tem um Battle Prefab configurado!");
                }

                // Inicia a batalha (agora com fade)
                CacheEnemyPositionsBeforeBattle();
                StartBattle();
            }

        }

    }
    #endregion



    private void StartBattle()
    {
        // Coloque aqui o NOME EXATO da sua cena de batalha
        GameManager.Instance.LoadSceneWithFade("BattleScene");
    }

    private void CacheEnemyPositionsBeforeBattle()
    {
        if (GameManager.Instance == null) return;

        EnemyAIController[] enemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (string.IsNullOrEmpty(e.enemyID)) continue;

            GameManager.Instance.enemyPositions[e.enemyID] = e.transform.position;
        }
    }

    private void RepelNearbyEnemies()
    {
        Vector2 playerPos = transform.position;

        EnemyAIController[] enemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);
        foreach (var ai in enemies)
        {
            if (ai == null || !ai.gameObject.activeInHierarchy) continue;

            // Se o inimigo estiver perto demais, empurra
            Vector2 enemyPos = ai.transform.position;
            float dist = Vector2.Distance(playerPos, enemyPos);

            if (dist >= enemySafeRadiusOnReturn) continue;

            Vector2 dir = (enemyPos - playerPos);
            if (dir.sqrMagnitude < 0.0001f)
                dir = Random.insideUnitCircle.normalized;

            Vector2 newPos = playerPos + dir.normalized * enemySafeRadiusOnReturn;

            // Move sem “teleport bug” de física
            Rigidbody2D rb = ai.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = newPos;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                ai.transform.position = newPos;
            }

            // Garante que ele não continue “agarrado” no player
            ai.StopChasing();
        }
    }
}
