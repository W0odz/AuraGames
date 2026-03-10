// PauseManager.cs (COM CONFIRMAÇÃO DE SAÍDA)
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject pausePanel;

    private bool isPaused = false;

    void Update()
    {
        if (GameManager.Instance.inputBloqueado) return;
        // Tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se a janela de confirmação estiver aberta, o ESC deve fechá-la primeiro
            if (isPaused)
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
        Time.timeScale = 1f;
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadSceneWithFade("TitleScreen");

    }

}