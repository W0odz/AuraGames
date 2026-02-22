using UnityEngine;

public class MonsterTarget : MonoBehaviour
{
    [Header("Configuração de Mira (debug)")]
    public float raioDeAcerto = 0.3f;

    void OnMouseDown()
    {
        // Deixe esse script apenas como debug/visualização.
        // O ataque agora deve começar pelo ActionOverlayAttackInput (UI), não por clique no monstro.
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        WeakPoint[] pontosFracos = FindObjectsByType<WeakPoint>(FindObjectsSortMode.None);
        foreach (WeakPoint ponto in pontosFracos)
        {
            float distancia = Vector2.Distance(mousePos, ponto.transform.position);
            Debug.Log($"Distância do clique para o Ponto Fraco: {distancia}");

            if (distancia <= raioDeAcerto)
            {
                ponto.ReceberClique();
                break;
            }
        }
    }
}