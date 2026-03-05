using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Indicador visual da Força de Vontade na BattleScene.
/// Círculo branco = tem | Cinza = não tem
/// Coloque este script em um GameObject com um componente Image.
/// </summary>
public class ForcaDeVontadeIndicador : MonoBehaviour
{
    [Header("Referência")]
    public Image circulo;

    [Header("Cores")]
    public Color corAtiva = Color.white;
    public Color corInativa = new Color(0.4f, 0.4f, 0.4f, 1f); // cinza

    void Update()
    {
        if (PlayerUnit.Instance == null || circulo == null) return;

        circulo.color = PlayerUnit.Instance.temForcaDeVontade
            ? corAtiva
            : corInativa;
    }
}