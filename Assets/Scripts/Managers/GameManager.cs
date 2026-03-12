using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Para usar Listas

public class GameManager : MonoBehaviour
{
    // --- Singleton (O Padr�o "Imortal") ---
    private static GameManager _instance;

    // 2. A "propriedade" p�blica inteligente
    public static GameManager Instance
    {
        get
        {
            // Se o _instance ainda n�o foi definido...
            if (_instance == null)
            {
                // 1. Tenta encontrar um na cena (caso j� exista)
                _instance = FindFirstObjectByType<GameManager>();

                // 2. Se n�o encontrar NENHUM na cena...
                if (_instance == null)
                {
                    // 3. ...Carrega o prefab da pasta "Resources"
                    GameObject gmPrefab = Resources.Load<GameObject>("GameManager");

                    if (gmPrefab != null)
                    {
                        GameObject gmInstance = Instantiate(gmPrefab);
                        _instance = gmInstance.GetComponent<GameManager>();
                        _instance.currentHP = _instance.maxHP;
                        _instance.currentMP = _instance.maxMP;
                    }
                    else
                    {
                        // Se falhar (ex: nome errado ou pasta errada)
                        Debug.LogError("ERRO FATAL: Prefab 'GameManager' n�o encontrado na pasta Resources!");
                    }
                }
            }

            // 4. Retorna a inst�ncia (que agora � garantido que existe)
            return _instance;
        }
    }

    [Header("Dados de Save")]
    // Esta � a nossa "�rea de transfer�ncia" (clipboard)
    public static GameData dataToCopy = null;

    [Header("Refer�ncias de Fade")]
    public Image fadeImage; // Arraste o FadeImage aqui
    public float fadeSpeed = 1.5f;

    [Header("Transi��o de Batalha")]
    public GameObject nextBattleEnemyPrefab; // O prefab que ser� spawnado na batalha
    public GameObject currentExplorationEnemyBattlePrefab; // battlePrefab do inimigo de explora��o que iniciou a batalha atual

    [Header("Dados Persistentes do Jogo")]
    public int currentSaveSlot = 1; // O slot que est� em uso
    public List<string> collectedItemIDs = new List<string>();
    public List<string> defeatedEnemyIDs = new List<string>();
    public string currentEnemyID;
    public string lastExplorationScene;
    public Vector3 playerReturnPosition; // Onde o jogador estava
    public bool isReturningFromBattle;   // Uma "bandeira" para saber se deve usar essa posi��o

    [Header("Player Stats & Level")]
    public string playerName = "Her�i"; // O campo para o nome
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Dados de Load")]
    public bool isLoadingSave = false; // "Estou carregando um save?"
    public Vector3 positionToLoad;     // "Para onde devo ir?"
    public string sceneToLoad;         // "Qual cena carregar?"

    [Header("Estados do Jogo")]
    public bool isBossBattle = false; // J� t�nhamos essa
    [SerializeField] private bool _triggerEndingOnLoad = false; // Vari�vel privada (aparece no inspector por causa do SerializeField)

    [Header("Combat Protection")]
    public float combatGraceDuration = 3f;
    public float combatGraceUntil = 0f;

    [Header("Enemy Return Safety")]
    public bool repelEnemiesOnReturn = false;
    public float enemySafeRadiusOnReturn = 3.0f;

    [Header("Enemy Persistence (Prototype)")]
    public Dictionary<string, Vector3> enemyPositions = new Dictionary<string, Vector3>();

    // Adiciona junto com as outras flags p�blicas
    public bool inputBloqueado = false;

    public bool IsInCombatGracePeriod()
    {
        return Time.unscaledTime < combatGraceUntil;
    }

    public void StartCombatGracePeriod()
    {
        combatGraceUntil = Time.unscaledTime + combatGraceDuration;
    }

    public bool triggerEndingOnLoad
    {
        get { return _triggerEndingOnLoad; }
        set
        {
            // O Debug vai nos dizer QUEM mudou o valor e QUANDO
            Debug.Log($"[GM DEBUG] 'triggerEndingOnLoad' mudou de {_triggerEndingOnLoad} para {value}.\nQuem fez isso? Veja a linha abaixo no stack trace.");

            _triggerEndingOnLoad = value;
        }
    }

    // Stats Base
    public int currentHP; // HP atual (para persistir entre batalhas)
    public int currentMP; // MP atual
    public int maxHP = 100;
    public int maxMP = 50;
    public int strength = 10;   // For�a (Ataque F�sico)
    public int speed = 5;       // Velocidade (ordem de turno, etc - n�o implementado ainda)
    public int resistance = 5;  // Resist�ncia (Defesa F�sica)
    public int will = 10;       // Vontade (Ataque M�gico)
    public int knowledge = 5;   // Conhecimento (Defesa M�gica)
    public int luck = 5;        // Sorte (Taxa de Cr�tico)
    public DadosArma armaEquipada;

    #region M�todos Unity
    void Awake()
    {
        Debug.Log("GAME MANAGER NASCEU! ID: " + gameObject.GetInstanceID());

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
            fadeImage.color = new Color(0, 0, 0, 0); // Come�a transparente
            fadeImage.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Fun��es de Save
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
            Debug.LogWarning("Arquivo de save n�o encontrado! Carregando novo jogo...");
            CreateNewGame("Her�i"); // Se n�o houver save, cria um novo
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
        collectedItemIDs = data.collectedItemIDs;

        // Guarda a posi��o e a cena para usar quando a cena carregar
        sceneToLoad = data.sceneName;
        positionToLoad = new Vector3(data.posX, data.posY, data.posZ);
        isLoadingSave = true; // Avisa o sistema que estamos carregando um save

        Debug.Log("Jogo carregado do Slot " + slot);
    }

    // Cria um novo jogo (usa valores padr�o)
    public void CreateNewGame(string playerNameInput)
    {
        GameData data = new GameData(); // Cria um cont�iner com valores padr�o

        if (!string.IsNullOrEmpty(playerNameInput))
        {
            data.playerName = playerNameInput;
        }
        else
        {
            data.playerName = "Her�i";
        }

        // Copia os dados padr�o para o GameManager
        playerName = data.playerName;
        playerLevel = data.playerLevel;
        currentXP = data.currentXP;
        xpToNextLevel = data.xpToNextLevel;
        maxHP = data.maxHP;
        maxMP = data.maxMP;
        currentHP = maxHP;
        currentMP = maxMP;
        strength = data.strength;
        speed = data.speed;
        resistance = data.resistance;
        will = data.will;
        knowledge = data.knowledge;
        luck = data.luck;
        defeatedEnemyIDs = data.defeatedEnemyIDs;
        collectedItemIDs = data.collectedItemIDs;

        Debug.Log("Novo jogo criado.");
    }

    // Salva os dados atuais do GameManager em um arquivo
    public void SaveCurrentGame()
    {
        // Cria um novo cont�iner
        GameData data = new GameData();

        // Copia os dados atuais do GameManager para o cont�iner
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
        data.collectedItemIDs = collectedItemIDs;

        // 1. Encontra o jogador na cena atual
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Salva a posi��o exata
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;
        }

        // Salva o nome da cena atual
        data.sceneName = SceneManager.GetActiveScene().name;

        // Manda o SaveSystem gravar o arquivo
        SaveSystem.SaveGame(data, currentSaveSlot);
    }
    #endregion

    #region Fun��es P�blicas de Transi��o

    // Chame isso em vez de SceneManager.LoadScene()
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeToSceneCoroutine(sceneName));
    }

    private IEnumerator FadeToSceneCoroutine(string sceneName)
    {
        bool isGoingToBattle = (sceneName == "BattleScene");

        // 1. Fade Out (Escurecer)
        yield return StartCoroutine(FadeOutCoroutine(isGoingToBattle));

        // 2. Carregar a Cena
        SceneManager.LoadScene(sceneName);
        if (repelEnemiesOnReturn)
        {
            RepelEnemiesNearPosition(playerReturnPosition, enemySafeRadiusOnReturn);
            repelEnemiesOnReturn = false;
        }

        // 3. Fade In (Clarear)
        // Damos um pequeno delay para a cena carregar
        yield return new WaitForSecondsRealtime(0.1f);
        yield return StartCoroutine(FadeInCoroutine());
    }

    private void RepelEnemiesNearPosition(Vector2 center, float radius)
    {
        EnemyAIController[] enemies = FindObjectsByType<EnemyAIController>(FindObjectsSortMode.None);

        foreach (var ai in enemies)
        {
            if (ai == null || !ai.gameObject.activeInHierarchy) continue;

            Vector2 enemyPos = ai.transform.position;
            float dist = Vector2.Distance(center, enemyPos);
            if (dist >= radius) continue;

            Vector2 dir = (enemyPos - center);
            if (dir.sqrMagnitude < 0.0001f)
                dir = Random.insideUnitCircle.normalized;

            Vector2 newPos = center + dir.normalized * radius;

            var rb = ai.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = newPos;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                ai.transform.position = newPos;
            }

            ai.StopChasing();
        }
    }

    // --- Coroutines de Fade ---
    private IEnumerator FadeOutCoroutine(bool useZoom)
    {
        float alpha = 0;
        fadeImage.gameObject.SetActive(true);

        // Vari�veis para o Zoom
        float startSize = 5f; // Valor padr�o caso n�o ache a c�mera
        float targetSize = 2.5f; // Zoom de 50%

        if (useZoom && Camera.main != null)
        {
            startSize = Camera.main.orthographicSize;
            targetSize = startSize * 0.6f; // Define o zoom final (60% do tamanho original)
        }

        while (alpha < 1)
        {
            alpha += Time.unscaledDeltaTime * fadeSpeed;

            // Aplica a cor preta
            fadeImage.color = new Color(0, 0, 0, alpha);

            // --- L�GICA DO ZOOM ---
            if (useZoom && Camera.main != null)
            {
                // Mathf.Lerp calcula o valor intermedi�rio entre A e B baseado no tempo (alpha)
                Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, alpha);

                // Opcional: Se voc� quisesse girar ou mover a c�mera, faria aqui tamb�m
            }
            // ---------------------

            yield return null;
        }
    }

    private IEnumerator FadeInCoroutine()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime * fadeSpeed;
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
        // subir de n�vel m�ltiplas vezes de uma vez
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        // (Aqui � onde futuramente chamaremos a UI da barra de XP)
    }

    private void LevelUp()
    {
        // Remove o XP necess�rio
        currentXP -= xpToNextLevel;
        playerLevel++;

        // Calcula o pr�ximo XP necess�rio (ex: 10% a mais que o anterior)
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

        // Cura o jogador totalmente ao subir de n�vel
        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log("LEVEL UP! N�vel " + playerLevel);
        // (Aqui chamaremos a UI de "Level Up!")
    }
    #endregion
}