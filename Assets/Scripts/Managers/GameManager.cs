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
    public static GameManager Instance
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
                        _instance.currentHP = _instance.maxHP;
                        _instance.currentMP = _instance.maxMP;
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

    [Tooltip("Image UI usada para exibir splash screens de transição de batalha. Deve ficar ABAIXO do fadeImage no Canvas (ordem de renderização menor).")]
    public UnityEngine.UI.Image battleTransitionImage;

    [Header("Transição de Batalha")]
    public GameObject nextBattleEnemyPrefab; // O prefab que será spawnado na batalha
    public GameObject currentExplorationEnemyBattlePrefab; // battlePrefab do inimigo de exploração que iniciou a batalha atual

    // Adicionar junto dos outros campos de estado (região "Estados do Jogo")

    [Header("Intro do Jogo")]
    [Tooltip("Se true, a cena da vila vai disparar o diálogo de intro e a primeira quest ao carregar.")]
    public bool triggerIntroOnLoad = false;

    [Header("Dados Persistentes do Jogo")]
    public int currentSaveSlot = 1; // O slot que está em uso
    public List<string> collectedItemIDs = new List<string>();
    public List<string> defeatedEnemyIDs = new List<string>();
    public List<string> removedCharacterIDs = new List<string>();
    public string currentEnemyID;
    public string lastExplorationScene;
    public Vector3 playerReturnPosition; // Onde o jogador estava
    public bool isReturningFromBattle;   // Uma "bandeira" para saber se deve usar essa posição
    public string pendingSpawnID = ""; // ID do SpawnPoint de destino na próxima cena

    [Header("Diálogo Pós-Vitória Pendente")]
    [Tooltip("DialogueAsset a ser disparado ao carregar a próxima cena, antes do fade in.")]
    public DialogueAsset dialogoPendente;
    [Tooltip("Cena destino para o diálogo pós-vitória.")]
    public string cenaDestinoPendente;

    [Header("Player Stats & Level")]
    public string playerName = "Herói"; // O campo para o nome
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Dados de Load")]
    public bool isLoadingSave = false; // "Estou carregando um save?"
    public Vector3 positionToLoad;     // "Para onde devo ir?"
    public string sceneToLoad;         // "Qual cena carregar?"

    [Header("Estados do Jogo")]
    public bool isBossBattle = false; // Já tínhamos essa
    [SerializeField] private bool _triggerEndingOnLoad = false; // Variável privada (aparece no inspector por causa do SerializeField)

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

    [Header("Diálogos Únicos Vistos")]
    public List<string> seenUniqueDialogues = new List<string>();

    [Header("Tutoriais Vistos")]
    public List<string> seenTutorialIDs = new List<string>();

    [Header("Transições Únicas Usadas")]
    public List<string> usedTransitionIDs = new List<string>();

    [Header("Itens de Cena Coletados")]
    // Chave: nome da cena | Valor: lista de paths hierárquicos dos itens já coletados
    // Não é serializado pelo Unity Inspector (Dictionary), mas é persistido via SaveCurrentGame/LoadGame.
    public Dictionary<string, List<string>> sceneCollectedItems = new Dictionary<string, List<string>>();

    // Adiciona junto com as outras flags públicas
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

    public bool TransicaoUsada(string transitionID)
        => usedTransitionIDs.Contains(transitionID);

    public void MarcarTransicaoUsada(string transitionID)
    {
        if (!usedTransitionIDs.Contains(transitionID))
            usedTransitionIDs.Add(transitionID);
    }

    public bool IsInCombatGracePeriod()
    {
        return Time.unscaledTime < combatGraceUntil;
    }

    public void StartCombatGracePeriod()
    {
        combatGraceUntil = Time.unscaledTime + combatGraceDuration;
    }

    // --- Helpers para itens de cena ---
    public bool ItemDeCenaFoiColetado(string sceneName, string itemPath)
    {
        return sceneCollectedItems.TryGetValue(sceneName, out var lista) && lista.Contains(itemPath);
    }

    public void MarcarItemDeCenaColetado(string sceneName, string itemPath)
    {
        if (!sceneCollectedItems.TryGetValue(sceneName, out var lista))
        {
            lista = new List<string>();
            sceneCollectedItems[sceneName] = lista;
        }
        if (!lista.Contains(itemPath))
            lista.Add(itemPath);
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
    public int maxHP = 11;
    public int maxMP = 50;
    public int strength = 7;   // Força (Ataque Físico)
    //public int speed = 5;       // Velocidade (ordem de turno, etc - não implementado ainda)
    public int resistance = 0;  // Resistência (Defesa Física)
    //public int will = 10;       // Vontade (Ataque Mágico)
    //public int knowledge = 5;   // Conhecimento (Defesa Mágica)
    //public int luck = 5;        // Sorte (Taxa de Crítico)
    public DadosArma armaEquipada;

    public void FadeComAcao(System.Action aoEscurecer)
    {
        StartCoroutine(FadeComAcaoCoroutine(aoEscurecer));
    }

    /// <summary>
    /// Exibe uma imagem de transição em tela cheia por pelo menos <paramref name="duracaoMinima"/> segundos,
    /// depois vai para a BattleScene com fade. Chamado por PlayerMovement quando o inimigo tem imagemTransicaoBatalha.
    /// </summary>
    public void IniciarTransicaoBatalha(Sprite imagemSplash, float duracaoMinima)
    {
        StartCoroutine(TransicaoBatalhaCoroutine(imagemSplash, duracaoMinima));
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
            CreateNewGame("Herói"); // Se não houver save, cria um novo
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
        resistance = data.resistance;
        //will = data.will;
        //knowledge = data.knowledge;
        //luck = data.luck;
        defeatedEnemyIDs = data.defeatedEnemyIDs;
        collectedItemIDs = data.collectedItemIDs;
        usedTransitionIDs = data.usedTransitionIDs ?? new List<string>();
        sceneToLoad = data.sceneName;
        positionToLoad = new Vector3(data.posX, data.posY, data.posZ);
        isLoadingSave = true; // Avisa o sistema que estamos carregando um save

        // Carrega itens de cena
        sceneCollectedItems = new Dictionary<string, List<string>>();
        if (data.sceneCollectedItems != null)
        {
            foreach (var entry in data.sceneCollectedItems)
                sceneCollectedItems[entry.sceneName] = entry.itemPaths ?? new List<string>();
        }

        LoadSceneWithFade(data.sceneName);

        Debug.Log("Jogo carregado do Slot " + slot);
    }

    // Cria um novo jogo (usa valores padrão)
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
        resistance = data.resistance;
        defeatedEnemyIDs = data.defeatedEnemyIDs;
        collectedItemIDs = data.collectedItemIDs;
        sceneCollectedItems = new Dictionary<string, List<string>>();

        // Reseta todas as flags de posicionamento para evitar que o jogador
        // seja spawnado na posição de batalha antiga ao iniciar um novo jogo
        isReturningFromBattle = false;
        isLoadingSave = false;
        playerReturnPosition = Vector3.zero;
        positionToLoad = Vector3.zero;
        pendingSpawnID = "";
        lastExplorationScene = "";

        // Sinaliza que deve rodar a intro ao carregar a cena
        triggerIntroOnLoad = true;

        Debug.Log("Novo jogo criado.");

        // Carrega a cena inicial
        LoadSceneWithFade(cenaInicial);
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
        data.resistance = resistance;
        //data.will = will;
        //data.knowledge = knowledge;
        //data.luck = luck;
        data.defeatedEnemyIDs = defeatedEnemyIDs;
        data.collectedItemIDs = collectedItemIDs;
        data.usedTransitionIDs = usedTransitionIDs;

        // Serializa o dicionário sceneCollectedItems
        data.sceneCollectedItems = new List<GameData.SceneItemEntry>();
        foreach (var kvp in sceneCollectedItems)
        {
            data.sceneCollectedItems.Add(new GameData.SceneItemEntry
            {
                sceneName = kvp.Key,
                itemPaths = new List<string>(kvp.Value)
            });
        }

        // 1. Encontra o jogador na cena atual
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Salva a posição exata
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

        // 2. Se há diálogo pendente, usa LoadSceneAsync para manter a tela preta
        //    enquanto a cena carrega, eliminando o flicker
        if (dialogoPendente != null)
        {
            // Salva tudo antes de limpar — cenaDestinoPendente é limpa aqui
            // mas precisa estar disponível pro EndDialogue verificar durante o diálogo
            DialogueAsset dialogoParaDisparar = dialogoPendente;
            string cenaFinalDestino = cenaDestinoPendente;
            string spawnFinal = pendingSpawnID;

            dialogoPendente = null;
            // NÃO limpa cenaDestinoPendente aqui — EndDialogue precisa checar ela

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            Time.timeScale = 1f;

            // Aguarda a cena carregar em background com a tela ainda preta
            while (op.progress < 0.9f)
                yield return null;

            // Ativa a cena — ainda com tela 100% preta
            op.allowSceneActivation = true;

            // Aguarda frames suficientes para a cena inicializar completamente
            yield return new WaitForSecondsRealtime(0.15f);

            if (repelEnemiesOnReturn)
            {
                RepelEnemiesNearPosition(playerReturnPosition, enemySafeRadiusOnReturn);
                repelEnemiesOnReturn = false;
            }

            // Clareia parcialmente (alpha 1 → 0.15) para o painel de diálogo
            // ficar visível sem expor a cena por baixo
            yield return StartCoroutine(FadeInParcialCoroutine(0.15f));

            // Dispara o diálogo com a tela semi-escurecida
            bool dialogoTerminou = false;
            DialogueRunner.Instance.StartDialogueImediato(dialogoParaDisparar, () =>
            {
                dialogoTerminou = true;
            });

            yield return new WaitUntil(() => dialogoTerminou);

            // Limpa agora que o diálogo terminou
            cenaDestinoPendente = null;

            // Se há cena destino pós-diálogo, navega pra lá com fade
            if (!string.IsNullOrEmpty(cenaFinalDestino))
            {
                yield return StartCoroutine(FadeOutCoroutine(false));

                if (!string.IsNullOrEmpty(spawnFinal))
                    pendingSpawnID = spawnFinal;

                SceneManager.LoadScene(cenaFinalDestino);
                Time.timeScale = 1f;
                yield return new WaitForSecondsRealtime(0.1f);
            }

            yield return StartCoroutine(FadeInCoroutine());
            yield break;
        }

        // 3. Sem diálogo pendente: comportamento original inalterado
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
        if (repelEnemiesOnReturn)
        {
            RepelEnemiesNearPosition(playerReturnPosition, enemySafeRadiusOnReturn);
            repelEnemiesOnReturn = false;
        }

        yield return new WaitForSecondsRealtime(0.1f);
        yield return StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator TransicaoBatalhaCoroutine(Sprite imagemSplash, float duracaoMinima)
    {
        // ── Etapa 1: Fade out da cena de exploração ───────────────────────
        yield return StartCoroutine(FadeOutCoroutine(false));

        // ── Etapa 2: No pico do preto — prepara e ativa a imagem splash ───
        if (battleTransitionImage != null)
        {
            battleTransitionImage.sprite = imagemSplash;
            battleTransitionImage.color = new Color(1f, 1f, 1f, 1f);
            battleTransitionImage.gameObject.SetActive(true);
        }

        // ── Etapa 3: Fade in — revela a splash ────────────────────────────
        yield return StartCoroutine(FadeInCoroutine());

        // ── Etapa 4: Aguarda o tempo mínimo garantido (mínimo 3 s) ────────
        float espera = Mathf.Max(duracaoMinima, 3f);
        yield return new WaitForSecondsRealtime(espera);

        // ── Etapa 5: Fade out da splash ───────────────────────────────────
        yield return StartCoroutine(FadeOutCoroutine(false));

        // ── Etapa 6: No pico do preto — desativa splash, carrega batalha ──
        if (battleTransitionImage != null)
            battleTransitionImage.gameObject.SetActive(false);

        SceneManager.LoadScene("BattleScene");
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.1f);

        // ── Etapa 7: Fade in da BattleScene ──────────────────────────────
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

        // Variáveis para o Zoom
        float startSize = 5f; // Valor padrão caso não ache a câmera
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

            // --- LÓGICA DO ZOOM ---
            if (useZoom && Camera.main != null)
            {
                // Mathf.Lerp calcula o valor intermediário entre A e B baseado no tempo (alpha)
                Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, alpha);

                // Opcional: Se você quisesse girar ou mover a câmera, faria aqui também
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

    public UnityEngine.UI.Image GetFadeImage() => fadeImage;

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

    public void LevelUp()
    {
        // Remove o XP necessário
        currentXP -= xpToNextLevel;
        playerLevel++;

        // Calcula o próximo XP necessário (ex: 10% a mais que o anterior)
        xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.5f);

        // Aumenta os Status!
        maxHP += 6;
        maxMP += 5;
        strength += 2;
        if (PlayerUnit.Instance != null) PlayerUnit.Instance.agility += 2;
        //resistance += 1;
        //will += 2;
        //knowledge += 1;
        //speed += 1;
        //luck += 1;

        // Cura o jogador totalmente ao subir de nível
        currentHP = maxHP;
        currentMP = maxMP;

        Debug.Log("LEVEL UP! Nível " + playerLevel);

        // Sincroniza PlayerUnit com o nível atualizado do GameManager
        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.playerLevel = playerLevel;
            PlayerUnit.Instance.currentXP = currentXP;
            PlayerUnit.Instance.xpToNextLevel = xpToNextLevel;
        }
        // (Aqui chamaremos a UI de "Level Up!")
    }
    #endregion
}