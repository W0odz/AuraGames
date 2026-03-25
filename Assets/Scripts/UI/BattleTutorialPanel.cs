using UnityEngine;
using TMPro;

public class BattleTutorialPanel : MonoBehaviour
{
    public static BattleTutorialPanel Instance;

    [Header("UI")]
    public TextMeshProUGUI textoTutorial;

    private System.Action _onClose;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            Fechar();
    }

    public void Mostrar(string texto, System.Action onClose)
    {
        _onClose = onClose;
        if (textoTutorial != null)
            textoTutorial.text = texto;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Fechar()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        _onClose?.Invoke();
        _onClose = null;
    }
}