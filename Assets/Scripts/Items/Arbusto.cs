using UnityEngine;

public class Arbusto : MonoBehaviour
{
    [Header("Item")]
    public DadosItem frutaItem;
    public int quantidade = 3;

    [Header("Visuais")]
    public Sprite spriteComFruta;
    public Sprite spriteSemFruta;

    [Header("Respawn (0 = não respawna)")]
    public float tempoRespawn = 0f;

    private SpriteRenderer sr;
    private bool foiColhido = false;
    private bool jogadorProximo = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (spriteComFruta != null && sr != null)
            sr.sprite = spriteComFruta;
    }

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
        if (!jogadorProximo || foiColhido) return;

        if (Input.GetKeyDown(KeyCode.E))
            Colher();
    }

    private void Colher()
    {
        if (frutaItem == null)
        {
            Debug.LogWarning("[Arbusto] frutaItem não atribuído!");
            return;
        }

        foiColhido = true;

        // Adiciona ao inventário
        InventoryManager.Instance.AdicionarItem(frutaItem, quantidade);

        // Troca visual
        if (sr != null && spriteSemFruta != null)
            sr.sprite = spriteSemFruta;

        Debug.Log($"[Arbusto] Coletado {quantidade}x {frutaItem.nomeItem}");

        // Respawn opcional
        if (tempoRespawn > 0f)
            StartCoroutine(Respawnar());
    }

    private System.Collections.IEnumerator Respawnar()
    {
        yield return new WaitForSeconds(tempoRespawn);

        foiColhido = false;

        if (sr != null && spriteComFruta != null)
            sr.sprite = spriteComFruta;

        Debug.Log("[Arbusto] Arbusto respawnou!");
    }
}