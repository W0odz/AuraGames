using UnityEngine;

/// <summary>
/// Coloque este script no GameObject do indicador de interação (ex: balão com [E])
/// como filho do NPC. Ele aparece quando o jogador entra no trigger e some quando sai.
/// A rotação é sempre mantida em 0 no espaço global, independente da rotação do NPC pai.
/// </summary>
public class InteractIndicator : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // Mantém a rotação world-space zerada, ignorando a rotação do pai
        transform.rotation = Quaternion.identity;
    }

    public void Mostrar() => gameObject.SetActive(true);
    public void Esconder() => gameObject.SetActive(false);
}
