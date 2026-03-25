using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Awake()
    {
        if (GameManager.Instance == null) return;

        string cena = SceneManager.GetActiveScene().name;
        string path = GetHierarchyPath();

        if (GameManager.Instance.ItemDeCenaFoiColetado(cena, path))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        string cena = SceneManager.GetActiveScene().name;
        string path = GetHierarchyPath();

        GameManager.Instance?.MarcarItemDeCenaColetado(cena, path);

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

    /// <summary>
    /// Retorna o caminho completo do objeto na hierarquia da cena.
    /// Ex: "Mapa/Itens/Pocao de Vida"
    /// Usado como ID automático — sem precisar configurar nada no Inspector.
    /// </summary>
    private string GetHierarchyPath()
    {
        string path = gameObject.name;
        Transform parent = transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}