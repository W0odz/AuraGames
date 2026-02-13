using UnityEngine;

public class BarreiraMagica : MonoBehaviour
{
    public string nomeDoItemNecessario = "Gema Azul";

    // Chamado quando o jogador interage ou encosta na barreira
    public void TentarAbrir()
    {
        // Pergunta ao Manager se o item está lá
        if (InventoryManager.Instance.TemItem(nomeDoItemNecessario))
        {
            AbrirCaminho();
        }
        else
        {
            Debug.Log("Você precisa da Gema Azul para passar!");
            // Aqui você pode disparar um texto na tela para o jogador
        }
    }

    void AbrirCaminho()
    {
        Debug.Log("Barreira dissipada!");
        // Você pode destruir o objeto ou tocar uma animação
        Destroy(gameObject);
    }

    // Exemplo se for por colisão
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TentarAbrir();
        }
    }
}