using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Painel de escolha da Força de Vontade.
/// Aparece quando o jogador morre com temForcaDeVontade = true.
/// Chame Mostrar(callback) para exibir o painel.
/// O callback retorna true se o jogador escolheu resistir, false se rendeu.
/// </summary>
public class ForcaDeVontadeUI : MonoBehaviour
{
    public static ForcaDeVontadeUI Instance;

    [Header("Referências")]
    public GameObject painel;
    public TextMeshProUGUI textoPergunta;
    public Button botaoResistir;
    public Button botaoRender;

    private Action<bool> _callback;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painel.SetActive(false);
    }

    /// <summary>
    /// Exibe o painel. O callback recebe true = resistir, false = render.
    /// </summary>
    public void Mostrar(Action<bool> callback)
    {
        _callback = callback;

        textoPergunta.text = "Esse golpe pode ser fatal...\nDeseja resistir?";

        botaoResistir.onClick.RemoveAllListeners();
        botaoRender.onClick.RemoveAllListeners();

        botaoResistir.onClick.AddListener(() => Responder(true));
        botaoRender.onClick.AddListener(() => Responder(false));

        painel.SetActive(true);
    }

    private void Responder(bool resistir)
    {
        painel.SetActive(false);
        _callback?.Invoke(resistir);
    }
}