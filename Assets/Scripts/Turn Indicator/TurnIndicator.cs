using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    private Transform target; // O alvo (jogador ou inimigo)
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Quão acima do alvo a seta deve ficar

    void Update()
    {
        // Se tivermos um alvo, siga a posição X e Y dele
        if (target != null)
        {
            // Usamos só X e Z se for 3D, ou X e Y se for 2D. 
            // Para sua câmera 2D, X e Y está correto.
            transform.position = target.position + offset;
        }
    }

    // Função para definir o alvo
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        gameObject.SetActive(true); // Mostra a seta
    }

    // Função para esconder
    public void Hide()
    {
        target = null;
        gameObject.SetActive(false); // Esconde a seta
    }
}