// PauseManager.cs (COM CONFIRMAÇÃO DE SAÍDA)
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject pausePanel;
    public GameObject quitConfirmationPanel; // Arraste o novo painel aqui

    [Header("Feedback Visual")]
    public TextMeshProUGUI saveButtonText;

    private bool isPaused = false;

    void Update()
    {
        // Tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se a janela de confirmação estiver aberta, o ESC deve fechá-la primeiro
            if (quitConfirmationPanel.activeSelf)
            {
                OnCancelQuitButton();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (!InventoryUIManager.Instance.isOpen)
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        quitConfirmationPanel.SetActive(false); // Garante que fecha tudo
        Time.timeScale = 1f;

        if (saveButtonText != null) saveButtonText.text = "Salvar Jogo";
    }

    public void OnSaveButton()
    {
        GameManager.Instance.SaveCurrentGame();
        if (saveButtonText != null) saveButtonText.text = "Salvo!";
    }

    // --- MUDANÇA AQUI: O botão "Voltar ao Menu" agora abre o painel ---
    public void OnMenuButton()
    {
        // Apenas abre a pergunta, não sai ainda
        quitConfirmationPanel.SetActive(true);

        // Opcional: Esconder o painel de pausa principal se quiser
        // pausePanel.SetActive(false); 
    }

    // --- NOVAS FUNÇÕES DA CONFIRMAÇÃO ---

    public void OnSaveAndQuitButton()
    {
        // 1. Salva
        GameManager.Instance.SaveCurrentGame();

        // 2. Descongela e Sai
        Time.timeScale = 1f;
        GameManager.Instance.LoadSceneWithFade("TitleScreen");
    }

    public void OnQuitNoSaveButton()
    {
        // 1. Apenas Descongela e Sai
        Time.timeScale = 1f;
        GameManager.Instance.LoadSceneWithFade("TitleScreen");
    }

    public void OnCancelQuitButton()
    {
        // Fecha a janela de confirmação e volta para o pause normal
        quitConfirmationPanel.SetActive(false);
        pausePanel.SetActive(true); // Garante que o menu de pausa esteja visível
    }
}