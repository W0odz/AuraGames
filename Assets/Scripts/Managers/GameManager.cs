using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Para usar Listas

public class GameManager : MonoBehaviour
{
    // --- Singleton (O Padrão "Imortal") ---
    public static GameManager instance;

    // --- Referências de Fade ---
    public Image fadeImage; // Arraste o FadeImage aqui
    public float fadeSpeed = 1.5f;

    // --- Dados Persistentes do Jogo ---
    public List<string> defeatedEnemyIDs = new List<string>();
    public string currentEnemyID;
    public string lastExplorationScene;

    [Header("Player Stats & Level")]
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    // Stats Base
    public int currentHP; // HP atual (para persistir entre batalhas)
    public int currentMP; // MP atual
    public int maxHP = 100;
    public int maxMP = 50;
    public int strength = 10;   // Força (Ataque Físico)
    public int speed = 5;       // Velocidade (ordem de turno, etc - não implementado ainda)
    public int resistance = 5;  // Resistência (Defesa Física)
    public int will = 10;       // Vontade (Ataque Mágico)
    public int knowledge = 5;   // Conhecimento (Defesa Mágica)
    public int luck = 5;        // Sorte (Taxa de Crítico)

    #region Métodos Unity
    void Awake()
    {
        // Configura o Singleton "Imortal"
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicia o HP e MP do jogador pela primeira vez
            currentHP = maxHP;
            currentMP = maxMP;

        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Garante que a tela de fade esteja pronta
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0); // Começa transparente
            fadeImage.gameObject.SetActive(true);
        }
    }
    #endregion

    # region Funções Públicas de Transição

    // Chame isso em vez de SceneManager.LoadScene()
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeToSceneCoroutine(sceneName));
    }

    private IEnumerator FadeToSceneCoroutine(string sceneName)
    {
        // 1. Fade Out (Escurecer)
        yield return StartCoroutine(FadeOutCoroutine());

        // 2. Carregar a Cena
        SceneManager.LoadScene(sceneName);

        // 3. Fade In (Clarear)
        // Damos um pequeno delay para a cena carregar
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(FadeInCoroutine());
    }

    // --- Coroutines de Fade ---
    private IEnumerator FadeOutCoroutine()
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeInCoroutine()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
    #endregion
}