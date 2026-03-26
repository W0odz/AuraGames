using UnityEngine;
using TMPro;

public class BattleTutorialPanel : MonoBehaviour
{
    public static BattleTutorialPanel Instance;

    [Header("Painel Visual (filho desativado)")]
    public GameObject painelUI;

    [Header("UI")]
    public TextMeshProUGUI textoTutorial;

    private System.Action _onClose;
    private bool _aberto;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (painelUI != null) painelUI.SetActive(false);
    }

    void Update()
    {
        if (!_aberto) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Fechar();
    }

    public void Mostrar(string texto, System.Action onClose)
    {
        _onClose = onClose;
        if (textoTutorial != null)
            textoTutorial.text = texto;

        if (painelUI != null) painelUI.SetActive(true);
        _aberto = true;
        Time.timeScale = 0f;
    }

    public void Fechar()
    {
        if (!_aberto) return;
        _aberto = false;

        if (painelUI != null) painelUI.SetActive(false);
        Time.timeScale = 1f;

        var cb = _onClose;
        _onClose = null;
        cb?.Invoke();
    }
}