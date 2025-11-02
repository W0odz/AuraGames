using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    private EnemyAIController aiController;

    void Start()
    {
        // Pega o script de IA que está no objeto "Pai"
        aiController = GetComponentInParent<EnemyAIController>();
    }

    // Agora, o círculo de detecção SÓ AVISA para começar
    // Ele não se importa mais se o jogador sair
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            aiController.StartChasing(other.transform);
        }
    }
}