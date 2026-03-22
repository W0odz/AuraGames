using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameInputPopup : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Campo de texto onde o jogador digita o nome.")]
    public TMP_InputField nameInputField;

    [Tooltip("Botão de confirmar / iniciar jogo.")]
    public Button botaoConfirmar;

    [Tooltip("Painel dos slots de save — ativado ao fechar este popup.")]
    public GameObject saveSlotsPanel;

    // Slot que receberá o novo jogo (definido por OpenPopup)
    private int _slotID;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (nameInputField != null)
            nameInputField.onValueChanged.AddListener(OnNomeAlterado);

        if (botaoConfirmar != null)
            botaoConfirmar.onClick.AddListener(OnConfirmarClicado);

        AtualizarBotao();
    }

    /// <summary>
    /// Abre o popup para criar um novo jogo no slot informado.
    /// </summary>
    public void OpenPopup(int slotID)
    {
        _slotID = slotID;

        if (nameInputField != null)
            nameInputField.text = "";

        AtualizarBotao();

        gameObject.SetActive(true);

        if (nameInputField != null)
            nameInputField.ActivateInputField();
    }

    /// <summary>
    /// Fecha o popup sem criar jogo (botão Cancelar / Voltar).
    /// </summary>
    public void OnCancelarClicado()
    {
        gameObject.SetActive(false);

        if (saveSlotsPanel != null)
            saveSlotsPanel.SetActive(true);
    }

    // ── Internos ───────────────────────────────────────────────────

    private void OnNomeAlterado(string novoTexto)
    {
        AtualizarBotao();
    }

    private void AtualizarBotao()
    {
        bool temNome = nameInputField != null &&
                       !string.IsNullOrWhiteSpace(nameInputField.text);

        if (botaoConfirmar != null)
            botaoConfirmar.interactable = temNome;
    }

    private void OnConfirmarClicado()
    {
        if (nameInputField == null) return;

        string nome = nameInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(nome)) return;

        gameObject.SetActive(false);

        GameManager.Instance.SetCurrentSlot(_slotID);
        GameManager.Instance.CreateNewGame(nome);
    }
}
