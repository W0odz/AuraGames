// PlayerMovement.cs (COMPLETO E ATUALIZADO PARA F�SICA)
using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para trocar de cena

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade do jogador

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Awake � chamado antes do Start
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Pega a refer�ncia do Rigidbody
    }

// Update � chamado uma vez por frame
void Update()
{
    // Pega o input do teclado (Setas ou WASD)
    float moveX = Input.GetAxisRaw("Horizontal");
    float moveY = Input.GetAxisRaw("Vertical");

    // .normalized garante que o movimento na diagonal
    // n�o seja mais r�pido que o normal.
    moveInput = new Vector2(moveX, moveY).normalized;
}

// FixedUpdate � chamado em um intervalo fixo (ideal para f�sica)
void FixedUpdate()
{
    // ANTES: rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    // DEPOIS: Definimos a velocidade, e deixamos a f�sica cuidar do resto
    rb.linearVelocity = moveInput * moveSpeed;
}

// Esta fun��o � chamada automaticamente pela Unity
// quando nosso colisor S�LIDO (do jogador) entra em um colisor "Is Trigger"
private void OnTriggerEnter2D(Collider2D other)
{
    // Verificamos se a coisa que tocamos tem a tag "Enemy"
    if (other.CompareTag("Enemy"))
    {
        // Se sim, inicie a batalha!
        // Desativa o inimigo na cena de explora��o
        other.gameObject.SetActive(false);

        // Carrega a cena de batalha
        StartBattle();
    }
}

private void StartBattle()
{
    // Coloque aqui o NOME EXATO da sua cena de batalha
    SceneManager.LoadScene("BattleScene");
}
}