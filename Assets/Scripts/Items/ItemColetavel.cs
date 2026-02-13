using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    public DadosItem itemParaDar;
    public int quantidade = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InventoryManager.Instance.AdicionarItem(itemParaDar, quantidade);
            Destroy(gameObject); // Simula a coleta
        }
    }
}