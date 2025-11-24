using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para trocar de cena

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade do jogador

    private Rigidbody2D rb;
    private Vector2 moveInput;


    #region Métodos Unity
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Pega a referência do Rigidbody
    }

    void Start()
    {
        // Verifica se o GameManager diz que estamos voltando de uma batalha
        if (GameManager.instance.isReturningFromBattle)
        {
            // Teleporta o jogador para a posição salva
            transform.position = GameManager.instance.playerReturnPosition;

            // Desliga a "bandeira" (para não teleportar de novo se você salvar/carregar depois)
            GameManager.instance.isReturningFromBattle = false;
        }
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
    #endregion

    // Esta função é chamada automaticamente pela Unity
    // quando nosso colisor SÓLIDO (do jogador) entra em um colisor "Is Trigger"
    private void OnTriggerEnter2D(Collider2D other)
    {
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

                //Congela tudo imediatamente
                EnemyAIController.FreezeAllEnemies();

                // Desativa o movimento do PRÓPRIO jogador também
                // para ele não continuar andando durante o fade
                this.enabled = false;
                rb.linearVelocity = Vector2.zero;

                // Salva qual inimigo estamos lutando
                GameManager.instance.currentEnemyID = ai.enemyID;

                // Salva de qual cena estamos vindo
                GameManager.instance.lastExplorationScene = SceneManager.GetActiveScene().name;

                // Antes de ir para a batalha, salva onde estamos
                GameManager.instance.playerReturnPosition = transform.position;
                GameManager.instance.isReturningFromBattle = true;

                // Pega o prefab de batalha do inimigo e manda pro GameManager
                if (ai.battlePrefab != null)
                {
                    GameManager.instance.nextBattleEnemyPrefab = ai.battlePrefab;
                }
                else
                {
                    Debug.LogError("O inimigo de exploração não tem um Battle Prefab configurado!");
                }

                // Inicia a batalha (agora com fade)
                StartBattle();
            }

        }

    }

    private void StartBattle()
    {
        // Coloque aqui o NOME EXATO da sua cena de batalha
        GameManager.instance.LoadSceneWithFade("BattleScene");
    }
}
