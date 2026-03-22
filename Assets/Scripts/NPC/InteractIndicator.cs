using UnityEngine;

/// <summary>
/// Coloque este script no GameObject do indicador de interação (ex: balão com [E])
/// como filho do NPC. Ele aparece quando o jogador entra no trigger e some quando sai.
/// </summary>
public class InteractIndicator : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Mostrar() => gameObject.SetActive(true);
    public void Esconder() => gameObject.SetActive(false);
}
