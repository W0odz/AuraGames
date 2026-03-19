using UnityEngine;

public class CollectFromObject : MonoBehaviour
{
    [Header("Item")]
    public DadosItem Item;
    public int quantidade = 1;

    [Header("Respawn (0 = não respawna)")]
    public float tempoRespawn = 0f;

    private bool foiColetado = false;
    private bool jogadorProximo = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorProximo = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorProximo = false;
    }

    private void Update()
    {
        if (!jogadorProximo || foiColetado) return;

        if (Input.GetKeyDown(KeyCode.E))
            Coletar();
    }

    private void Coletar()
    {
        if (Item == null)
        {
            Debug.LogWarning("[Arbusto] frutaItem não atribuído!");
            return;
        }

        foiColetado = true;

        // Adiciona ao inventário
        InventoryManager.Instance.AdicionarItem(Item, quantidade);

        Debug.Log($"[Arbusto] Coletado {quantidade}x {Item.nomeItem}");

        // Respawn opcional
        if (tempoRespawn > 0f)
            StartCoroutine(Respawnar());
    }

    private System.Collections.IEnumerator Respawnar()
    {
        yield return new WaitForSeconds(tempoRespawn);

        foiColetado = false;
    }
}
