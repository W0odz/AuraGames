using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    [Header("Item")]
    public DadosItem itemParaDar;
    public int quantidade = 1;

    [Header("Alterar Velocidade do Jogador (opcional)")]
    [Tooltip("Se true, altera a velocidade do jogador ao coletar este item.")]
    public bool alterarVelocidade = false;

    [Tooltip("Nova velocidade do jogador após coletar.")]
    public float novaVelocidade = 6f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        InventoryManager.Instance.AdicionarItem(itemParaDar, quantidade);

        if (alterarVelocidade)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                pm.moveSpeed = novaVelocidade;
                Debug.Log($"[ItemColetavel] Velocidade do jogador alterada para {novaVelocidade}.");
            }
        }

        Destroy(gameObject);
    }
}