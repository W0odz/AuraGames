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

    // Adicionar junto dos outros campos de estado (região "Estados do Jogo")

    [Header("Intro do Jogo")]
    [Tooltip("Se true, a cena da vila vai disparar o diálogo de intro e a primeira quest ao carregar.")]
    public bool triggerIntroOnLoad = false;

    [Header("Dados Persistentes do Jogo")]
    public int currentSaveSlot = 1; // O slot que est� em uso
    public List<string> collectedItemIDs = new List<string>();
    public List<string> defeatedEnemyIDs = new List<string>();
    public List<string> removedCharacterIDs = new List<string>();
    public string currentEnemyID;
    public string lastExplorationScene;
    public Vector3 playerReturnPosition; // Onde o jogador estava
    public bool isReturningFromBattle;   // Uma "bandeira" para saber se deve usar essa posi��o
    public string pendingSpawnID = ""; // ID do SpawnPoint de destino na próxima cena

    [Header("Diálogo Pós-Vitória Pendente")]
    [Tooltip("DialogueAsset a ser disparado ao carregar a próxima cena, antes do fade in.")]
    public DialogueAsset dialogoPendente;
    [Tooltip("Cena destino para o diálogo pós-vitória.")]
    public string cenaDestinoPendente;

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

    [Header("Batalha — Fundo")]
    [Tooltip("Sprite do fundo que será usado na BattleScene. Definido automaticamente pela cena de exploração.")]
    public Sprite battleBackground;

    [Header("Enemy Return Safety")]
    public bool repelEnemiesOnReturn = false;
    public float enemySafeRadiusOnReturn = 3.0f;

    [Header("Enemy Persistence (Prototype)")]
    public Dictionary<string, Vector3> enemyPositions = new Dictionary<string, Vector3>();

    [Header("Diálogos Únicos Vistros")]
    public List<string> seenUniqueDialogues = new List<string>();

    // Adiciona junto com as outras flags p�blicas
    public bool inputBloqueado = false;

    private bool _isShuttingDown = false;

    /// <summary>
    /// True quando o jogo está em processo de encerramento.
    /// Use para impedir que sistemas iniciem novas ações durante o shutdown.
    /// </summary>
    public bool IsShuttingDown => _isShuttingDown;

    public bool DialogoUnicoVisto(string npcID)
    => seenUniqueDialogues.Contains(npcID);

    public void MarcarDialogoUnicoVisto(string npcID)
    {
        if(!seenUniqueDialogues.Contains(npcID))
            seenUniqueDialogues.Add(npcID);
    }

    public bool PersonagemRemovido(string characterId)
        => removedCharacterIDs.Contains(characterId);

    public void MarcarPersonagemRemovido(string characterId)
    {
        if (!removedCharacterIDs.Contains(characterId))
            removedCharacterIDs.Add(characterId);
    }

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
    //public int speed = 5;       // Velocidade (ordem de turno, etc - n�o implementado ainda)
    //public int resistance = 5;  // Resist�ncia (Defesa F�sica)
    //public int will = 10;       // Vontade (Ataque M�gico)
    //public int knowledge = 5;   // Conhecimento (Defesa M�gica)
    //public int luck = 5;        // Sorte (Taxa de Cr�tico)
    public DadosArma armaEquipada;

    public void FadeComAcao(System.Action aoEscurecer)
    {
        StartCoroutine(FadeComAcaoCoroutine(aoEscurecer));
    }

    private IEnumerator FadeComAcaoCoroutine(System.Action aoEscurecer)
    {
        // Fade Out
        yield return StartCoroutine(FadeOutCoroutine(false));

        // Executa a ação no pico do preto (ex: ativar fundo + abrir diálogo)
        aoEscurecer?.Invoke();

        // Pequena pausa pra UI atualizar antes de clarear
        yield return new WaitForSecondsRealtime(0.05f);

        // Fade In
        yield return StartCoroutine(FadeInCoroutine());
    }

    #region Métodos Unity
    private void OnApplicationQuit()
    {
        if (_isShuttingDown) return;
        _isShuttingDown = true;

        // Bloqueia qualquer input — nada mais deve acontecer
        inputBloqueado = true;

        // NÃO chama SaveCurrentGame() — save é exclusivamente via Fogueira.
        // Salvar aqui quebraria a mecânica intencional de save limitado.

        // Garante que PlayerPrefs (volumes de áudio, configurações) sejam gravados em disco
        PlayerPrefs.Save();

        Debug.Log("[GameManager] Graceful shutdown concluído. Progresso não salvo desde a última Fogueira foi descartado (comportamento esperado).");
    }

    void Awake()
    {
        Debug.Log("GAME MANAGER NASCEU! ID: " + gameObject.GetInstanceID());

        // Configura o Singleton "Imortal"
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            // Reset defensivo do timeScale (caso uma cena anterior tenha deixado travado)
            Time.timeScale = 1f;
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
        //speed = data.speed;
        //resistance = data.resistance;
        //will = data.will;
        //knowledge = data.knowledge;
        //luck = data.luck;
        defeatedEnemyIDs = data.defeatedEnemyIDs;
        collectedItemIDs = data.collectedItemIDs;

        // Guarda a posi��o e a cena para usar quando a cena carregar
        sceneToLoad = data.sceneName;
        positionToLoad = new Vector3(data.posX, data.posY, data.posZ);
        isLoadingSave = true; // Avisa o sistema que estamos carregando um save

        LoadSceneWithFade(data.sceneName);

        Debug.Log("Jogo carregado do Slot " + slot);
    }

    // Cria um novo jogo (usa valores padr�o)
    public void CreateNewGame(string playerNameInput, string cenaInicial = "Vila_01")
    {
        GameData data = new GameData();

        if (!string.IsNullOrEmpty(playerNameInput))
            data.playerName = playerNameInput;
        else
            data.playerName = "Herói";

        // Copia os dados padrão para o GameManager
        playerName = data.playerName;
        playerLevel = data.playerLevel;
        currentXP = data.currentXP;
        xpToNextLevel = data.xpToNextLevel;
        maxHP = data.maxHP;
        maxMP = data.maxMP;
        currentHP = maxHP;
        currentMP = maxMP;
        strength = data.strength;
        defeatedEnemyIDs = data.defeatedEnemyIDs;
        collectedItemIDs = data.collectedItemIDs;

        // Sinaliza que deve rodar a intro ao carregar a cena
        triggerIntroOnLoad = true;

        Debug.Log("Novo jogo criado.");

        // Carrega a cena inicial
        LoadSceneWithFade(cenaInicial);
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
        //data.speed = speed;
        //data.resistance = resistance;
        //data.will = will;
        //data.knowledge = knowledge;
        //data.luck = luck;
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

    #region Funções Públicas de Transição

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
        Time.timeScale = 1f; // Garante reset do timeScale em qualquer transição de cena
        if (repelEnemiesOnReturn)
        {
            RepelEnemiesNearPosition(playerReturnPosition, enemySafeRadiusOnReturn);
            repelEnemiesOnReturn = false;
        }

        // Pequeno delay para a cena carregar
        yield return new WaitForSecondsRealtime(0.1f);

        // 3. Verifica se há diálogo pendente para disparar antes do fade in
        if (dialogoPendente != null)
        {
            DialogueAsset dialogoParaDisparar = dialogoPendente;
            dialogoPendente = null;
            cenaDestinoPendente = null;

            // Clareia apenas parcialmente (alpha 1 → 0.15) para o painel de diálogo
            // ficar visível sem expor a cena por baixo
            yield return StartCoroutine(FadeInParcialCoroutine(0.15f));

            // Dispara o diálogo com a tela semi-escurecida
            bool dialogoTerminou = false;
            DialogueRunner.Instance.StartDialogueImediato(dialogoParaDisparar, () =>
            {
                dialogoTerminou = true;
            });

            yield return new WaitUntil(() => dialogoTerminou);

            // Após o diálogo terminar, clareia a tela completamente
            yield return StartCoroutine(FadeInCoroutine());
            yield break;
        }

        // 4. Fade In (Clarear) — acontece imediatamente se não há diálogo pendente
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

    /// <summary>
    /// Clareia a tela do preto total (alpha=1) até o alpha alvo informado.
    /// Usado para deixar o painel de diálogo visível sem expor a cena por baixo.
    /// </summary>
    private IEnumerator FadeInParcialCoroutine(float alphaAlvo)
    {
        float alpha = 1f;
        while (alpha > alphaAlvo)
        {
            alpha -= Time.unscaledDeltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, Mathf.Max(alpha, alphaAlvo));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, alphaAlvo);
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
        maxHP += 6;
        maxMP += 5;
        strength += 2;
        PlayerUnit.Instance.agility += 2;
        //resistance += 1;
        //will += 2;
        //knowledge += 1;
        //speed += 1;
        //luck += 1;

        // Cura o jogador totalmente ao subir de n�vel
        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log("LEVEL UP! N�vel " + playerLevel);
        // (Aqui chamaremos a UI de "Level Up!")
    }
    #endregion
}