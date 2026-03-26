using UnityEngine;

/// <summary>
/// Controla a visibilidade do InteractIndicator em objetos interagíveis do cenário
/// (itens, portas, etc.) usando a mesma lógica do NpcInteractable:
///   - Aparece quando o jogador entra no trigger E não há diálogo ativo
///   - Some quando o jogador sai do trigger ou diálogo abre
/// Não contém lógica de interação — cada objeto implementa a sua própria.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ObjectInteractIndicatorController : MonoBehaviour
{
    [Header("Indicador de interação")]
    [Tooltip("Arraste aqui o GameObject filho com o ícone [E].")]
    public InteractIndicator indicador;

    private bool playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (indicador != null) indicador.Esconder();
        }
    }

    private void Update()
    {
        if (indicador == null) return;

        bool inputBloqueado = GameManager.Instance != null && GameManager.Instance.inputBloqueado;
        bool dialogoAtivo = DialogueRunner.Instance != null && DialogueRunner.Instance.IsDialogueActive;

        bool shouldShow = playerNearby && !inputBloqueado && !dialogoAtivo;

        if (shouldShow && !indicador.gameObject.activeSelf)
            indicador.Mostrar();
        else if (!shouldShow && indicador.gameObject.activeSelf)
            indicador.Esconder();
    }
}