using UnityEngine;

public class MonsterTarget : MonoBehaviour
{
    [Header("Configuração de Mira")]
    public float raioDeAcerto = 0.3f;

    void OnMouseDown()
    {
        if (BattleSystem.Instance.state != BattleState.BUSY) return;
        BattleSystem.Instance.state = BattleState.ENEMYTURN;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float bonusAtaque = 1.0f;

        WeakPoint[] pontosFracos = FindObjectsByType<WeakPoint>(FindObjectsSortMode.None);

        foreach (WeakPoint ponto in pontosFracos)
        {
            float distancia = Vector2.Distance(mousePos, ponto.transform.position);

            Debug.Log($"Distância do clique para o Ponto Fraco: {distancia}");

            if (distancia <= raioDeAcerto)
            {
                if (ponto.ReceberClique())
                {
                    bonusAtaque = 2.0f;
                }
                break;
            }
        }

        // Envia o multiplicador e a posição física do clique para o AttackManager
        AttackManager.Instance.IniciarSequenciaDeAtaque(bonusAtaque, mousePos);
    }
}