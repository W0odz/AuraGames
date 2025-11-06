using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Para usar Listas

public class GameManager : MonoBehaviour
{
    // --- Singleton (O Padrão "Imortal") ---
    private static GameManager _instance;

    // 2. A "propriedade" pública inteligente
    public static GameManager instance
    {
        get
        {
            // Se o _instance ainda não foi definido...
            if (_instance == null)
            {
                // 1. Tenta encontrar um na cena (caso já exista)
                _instance = FindFirstObjectByType<GameManager>();

                // 2. Se não encontrar NENHUM na cena...
                if (_instance == null)
                {
                    // 3. ...Carrega o prefab da pasta "Resources"
                    GameObject gmPrefab = Resources.Load<GameObject>("GameManager");

                    if (gmPrefab != null)
                    {
                        GameObject gmInstance = Instantiate(gmPrefab);
                        _instance = gmInstance.GetComponent<GameManager>();
                    }
                    else
                    {
                        // Se falhar (ex: nome errado ou pasta errada)
                        Debug.LogError("ERRO FATAL: Prefab 'GameManager' não encontrado na pasta Resources!");
                    }
                }
            }

            // 4. Retorna a instância (que agora é garantido que existe)
            return _instance;
        }
    }

    [Header("Dados de Save")]
    // Esta é a nossa "área de transferência" (clipboard)
    public static GameData dataToCopy = null;

    [Header("Referências de Fade")]
    public Image fadeImage; // Arraste o FadeImage aqui
    public float fadeSpeed = 1.5f;

    [Header("Dados Persistentes do Jogo")]
    public int currentSaveSlot = 1; // O slot que está em uso
    public List<string> defeatedEnemyIDs = new List<string>();
    public string currentEnemyID;
    public string lastExplorationScene;

    [Header("Player Stats & Level")]
    public string playerName = "Herói"; // O campo para o nome
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
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else if (_instance != this)
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

    #region Funções de Save
    public void SetCurrentSlot(int slot)
    {
        currentSaveSlot = slot;
    }

    // Carrega os dados do arquivo para o GameManager
    public void LoadGame(int slot)
    {
        GameData data = SaveSystem.LoadGame(slot);

        if (data == null)
        {
            Debug.LogWarning("Arquivo de save não encontrado! Carregando novo jogo...");
            CreateNewGame(); // Se não houver save, cria um novo
            return;
        }

        // Copia os dados do arquivo para o GameManager
        playerName = data.playerName;
        playerLevel = data.playerLevel;
        currentXP = data.currentXP;
        xpToNextLevel = data.xpToNextLevel;
        currentHP = data.currentHP;
        currentMP = data.currentMP;
        maxHP = data.maxHP;
        maxMP = data.maxMP;
        strength = data.strength;
        speed = data.speed;
        resistance = data.resistance;
        will = data.will;
        knowledge = data.knowledge;
        luck = data.luck;
        defeatedEnemyIDs = data.defeatedEnemyIDs;

        Debug.Log("Jogo carregado do Slot " + slot);
    }

    // Cria um novo jogo (usa valores padrão)
    public void CreateNewGame()
    {
        GameData data = new GameData(); // Cria um contêiner com valores padrão

        // Copia os dados padrão para o GameManager
        playerName = data.playerName;
        playerLevel = data.playerLevel;
        currentXP = data.currentXP;
        xpToNextLevel = data.xpToNextLevel;
        currentHP = data.currentHP;
        currentMP = data.currentMP;
        maxHP = data.maxHP;
        maxMP = data.maxMP;
        strength = data.strength;
        speed = data.speed;
        resistance = data.resistance;
        will = data.will;
        knowledge = data.knowledge;
        luck = data.luck;
        defeatedEnemyIDs = data.defeatedEnemyIDs;

        Debug.Log("Novo jogo criado.");
    }

    // Salva os dados atuais do GameManager em um arquivo
    public void SaveCurrentGame()
    {
        // Cria um novo contêiner
        GameData data = new GameData();

        // Copia os dados atuais do GameManager para o contêiner
        data.playerName = playerName;
        data.playerLevel = playerLevel;
        data.currentXP = currentXP;
        data.xpToNextLevel = xpToNextLevel;
        data.currentHP = currentHP;
        data.currentMP = currentMP;
        data.maxHP = maxHP;
        data.maxMP = maxMP;
        data.strength = strength;
        data.speed = speed;
        data.resistance = resistance;
        data.will = will;
        data.knowledge = knowledge;
        data.luck = luck;
        data.defeatedEnemyIDs = defeatedEnemyIDs;

        // Manda o SaveSystem gravar o arquivo
        SaveSystem.SaveGame(data, currentSaveSlot);
    }
    #endregion

#region Funções Públicas de Transição

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

    #region Concedimento de Xp
    // Chamado pelo BattleSystem quando o jogador vence
    public void GainXP(int xpGained)
    {
        currentXP += xpGained;

        // Loop 'while' caso o jogador ganhe XP suficiente para
        // subir de nível múltiplas vezes de uma vez
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        // (Aqui é onde futuramente chamaremos a UI da barra de XP)
    }

    private void LevelUp()
    {
        // Remove o XP necessário
        currentXP -= xpToNextLevel;
        playerLevel++;

        // Calcula o próximo XP necessário (ex: 10% a mais que o anterior)
        xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.5f);

        // Aumenta os Status!
        maxHP += 10;
        maxMP += 5;
        strength += 2;
        resistance += 1;
        will += 2;
        knowledge += 1;
        speed += 1;
        luck += 1;

        // Cura o jogador totalmente ao subir de nível
        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log("LEVEL UP! Nível " + playerLevel);
        // (Aqui chamaremos a UI de "Level Up!")
    }
    #endregion
}