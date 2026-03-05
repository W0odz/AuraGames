using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanelUI : MonoBehaviour
{
    public static GameOverPanelUI Instance;

    [Header("Referências")]
    public GameObject painel;
    public TextMeshProUGUI textoMensagem;
    public Button botaoSair;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painel.SetActive(false);
    }

    void Start()
    {
        botaoSair.onClick.AddListener(VoltarAoMenu);
    }

    public void Mostrar()
    {
        painel.SetActive(true);

        if (textoMensagem != null)
            textoMensagem.text = "Você foi derrotado...";
    }

    public void VoltarAoMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithFade("TitleScreen");
        else
        {
            // Fallback direto se GameManager não existir
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
        }
    }
}