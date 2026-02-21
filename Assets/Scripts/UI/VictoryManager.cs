using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    public GameObject victoryPanel;

    void Start()
    {
        // Garante que comece fechado e o tempo rodando
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Verifica se o GameManager mandou ativar a vitória
        if (GameManager.Instance.triggerEndingOnLoad)
        {
            StartCoroutine(ShowVictoryRoutine());
        }
    }

    IEnumerator ShowVictoryRoutine()
    {
        // 1. Espera o Fade In terminar (visual)
        yield return new WaitForSeconds(1.5f);

        // 2. Mostra o painel
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // --- MUDANÇA AQUI: CONGELA O JOGO ---
            Time.timeScale = 0f;
            // ------------------------------------
        }

        GameManager.Instance.triggerEndingOnLoad = false;
        GameManager.Instance.SaveCurrentGame();
    }

    // Função para o Botão "Voltar ao Menu"
    public void OnMenuButton()
    {
        // --- MUDANÇA AQUI: DESCONGELA ANTES DE SAIR ---
        // Se não fizer isso, o Fade Out não roda e a próxima cena trava!
        Time.timeScale = 1f;
        // ----------------------------------------------

        GameManager.Instance.LoadSceneWithFade("TitleScreen");
    }
}