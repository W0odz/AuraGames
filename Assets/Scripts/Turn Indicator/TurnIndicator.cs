using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    [Header("Configurações")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Altura acima da cabeça
    public Color playerTurnColor = Color.cyan;
    public Color enemyTurnColor = Color.red;

    private Transform target;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Se não achar no filho, tenta no próprio objeto (fallback)
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Segue o alvo se ele existir
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    // Função chamada pelo BattleSystem
    public void SetTarget(Transform newTarget, bool isPlayerTurn)
    {
        target = newTarget;

        // Troca a cor dependendo de quem é a vez
        if (spriteRenderer != null)
        {
            if (isPlayerTurn)
                spriteRenderer.color = playerTurnColor;
            else
                spriteRenderer.color = enemyTurnColor;
        }

        gameObject.SetActive(true); // Mostra o indicador
    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false); // Esconde o indicador
    }
}