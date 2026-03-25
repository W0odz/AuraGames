using UnityEngine;
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, TARGETING, WON, LOST, BUSY }

[System.Serializable]
public class DialogoPosBatalha
{
    [Tooltip("ID do inimigo (currentEnemyID do GameManager) que dispara este diálogo.")]
    public string enemyId;
    [Tooltip("DialogueAsset a ser exibido ao vencer contra esse inimigo.")]
    public DialogueAsset dialogo;
    [Tooltip("Cena onde o diálogo será exibido (ex: floresta).")]
    public string cenaDialogo;
    [Tooltip("Cena para onde ir após o diálogo terminar (ex: Resistencia).")]
    public string cenaAposDialogo;
    [Tooltip("ID do SpawnPoint na cena pós-diálogo onde o jogador vai aparecer.")]
    public string spawnIdDestino;
}

[System.Serializable]
public class TutorialDeBatalha
{
    [Tooltip("Prefab do inimigo que dispara este tutorial.")]
    public GameObject enemyPrefab;
    [Tooltip("Texto exibido no painel de tutorial ao enfrentar este inimigo pela primeira vez.")]
    [TextArea(3, 8)]
    public string textoTutorial;
}

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    [Header("Painéis de UI")]
    public GameObject dialoguePanel;
    public GameObject commandsPanel;

    [Header("Telas de Vitória")]
    public GameObject xpPanel;
    public UnityEngine.UI.Slider xpSlider;
    public TextMeshProUGUI levelText;

    public string nomeCenaMapa = "ExplorationScene";

    [Header("Configurações do Inimigo")]
    public GameObject enemyPrefab;
    public Transform enemyBattleStation;
    public EnemyUnit enemyUnit;

    [Header("Dados do Jogador")]
    public PlayerUnit playerUnit;

    [Header("Interface (HUD)")]
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    public TextMeshProUGUI dialogueText;

    [Header("HUD do Inimigo (Fade rápido)")]
    public CanvasGroup enemyHudCanvasGroup;

    [Header("Durações")]
    public float duracaoFadeInimigo = 1f;
    public float duracaoFadeHudInimigo = 0.6f;

    [Header("Pausas entre etapas")]
    public float pausaAntesDeSumirInimigo = 1.5f;
    public float pausaAposFadeInimigo = 1.5f;
    public float pausaAposXP = 1.5f;

    [Header("Diálogos Pós-Vitória")]
    public DialogoPosBatalha[] dialogosPosVitoria;

    [Header("Tutorial de Batalha")]
    [Tooltip("Associa prefabs de inimigo a textos de tutorial. Cada entrada é exibida apenas uma vez.")]
    public TutorialDeBatalha[] tutoriaisDeBatalha;

    [Header("Batalha Especial")]
    [Tooltip("ID do inimigo que termina a batalha quando chega na metade do HP.")]
    public string halfHpVictoryEnemyID;
    [Tooltip("ID do inimigo que não permite fuga.")]
    public string noFleeEnemyID;

    public BattleState state;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // Espera o PlayerUnit estar disponível (máx. 3 segundos)
        float timeout = 3f;
        while (PlayerUnit.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (PlayerUnit.Instance == null)
        {
            Debug.LogError("[BattleSystem] PlayerUnit.Instance é NULL após espera. Verifique se o PlayerUnit está na cena de exploração.");
            yield break;
        }

        playerUnit = PlayerUnit.Instance;
        playerUnit.InicializarUnidade();

        // Usa o prefab do inimigo que colidiu na exploração — fallback pro campo do Inspector
        GameObject prefabParaInstanciar = GameManager.Instance?.nextBattleEnemyPrefab ?? enemyPrefab;

        if (prefabParaInstanciar == null)
        {
            Debug.LogError("[BattleSystem] Nenhum prefab de inimigo definido. Atribua um no Inspector ou verifique o PlayerMovement.");
            yield break;
        }

        GameObject enemyGO = Instantiate(prefabParaInstanciar, enemyBattleStation);
        enemyGO.transform.localPosition = Vector3.zero;
        enemyUnit = enemyGO.GetComponent<EnemyUnit>();

        // Limpa o prefab do GameManager após usar
        if (GameManager.Instance != null)
            GameManager.Instance.nextBattleEnemyPrefab = null;

        if (enemyUnit == null)
        {
            Debug.LogError("[BattleSystem] prefab instanciado não tem componente EnemyUnit.");
            yield break;
        }

        if (QuestManager.Instance != null)
        {
            string enemyId = GameManager.Instance?.currentEnemyID ?? enemyUnit.unitName;
            QuestManager.Instance.NotificarInicioCombate(enemyId);
        }

        if (dialogueText != null)
            dialogueText.text = "Um " + enemyUnit.unitName + " bloqueia seu caminho...";

        if (playerHUD != null) playerHUD.SetHUD(playerUnit);
        else Debug.LogError("[BattleSystem] playerHUD não está atribuído no Inspector.");

        if (enemyHUD != null) enemyHUD.SetHUD(enemyUnit);
        else Debug.LogError("[BattleSystem] enemyHUD não está atribuído no Inspector.");

        yield return new WaitForSeconds(2f);

        if (AttackManager.Instance == null)
        {
            Debug.LogError("ERRO: O objeto AttackManager não foi encontrado na cena!");
            yield break;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError("ERRO: O EquipmentManager não foi encontrado na cena!");
            yield break;
        }

        if (AttackManager.Instance != null && EquipmentManager.Instance != null)
        {
            AttackManager.Instance.armaAtual = (DadosArma)EquipmentManager.Instance.currentEquipment[0];
            Debug.Log("Sucesso: AttackManager recebeu a arma " + AttackManager.Instance.armaAtual.name);
        }

        // Aguarda o BattleTutorialPanel inicializar (máx. 2 segundos)
        float tutorialTimeout = 2f;
        while (BattleTutorialPanel.Instance == null && tutorialTimeout > 0f)
        {
            tutorialTimeout -= Time.deltaTime;
            yield return null;
        }

        // Tutorial baseado em prefab
        TutorialDeBatalha tutorialParaExibir = null;
        GameObject prefabUsado = GameManager.Instance?.currentExplorationEnemyBattlePrefab;

        string prefabUsadoName = prefabUsado != null ? prefabUsado.name.Replace(" (Clone)", "").Trim() : null;

        if (prefabUsadoName != null && tutoriaisDeBatalha != null && GameManager.Instance != null)
        {
            foreach (var t in tutoriaisDeBatalha)
            {
                if (t.enemyPrefab != null
                    && t.enemyPrefab.name == prefabUsadoName
                    && !GameManager.Instance.seenTutorialIDs.Contains(prefabUsadoName))
                {
                    tutorialParaExibir = t;
                    break;
                }
            }
        }

        if (tutorialParaExibir != null && BattleTutorialPanel.Instance != null)
        {
            GameManager.Instance.seenTutorialIDs.Add(prefabUsadoName);
            BattleTutorialPanel.Instance.Mostrar(tutorialParaExibir.textoTutorial, () =>
            {
                state = BattleState.PLAYERTURN;
                PlayerTurn();
            });
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void PlayerTurn()
    {
        playerUnit.TickDebuffsOnPlayerTurnStart();

        if (playerUnit.HasDebuff(DebuffType.Stun))
        {
            if (dialogueText != null)
                dialogueText.text = $"{playerUnit.unitName} está atordoado e perde o turno!";

            StartCoroutine(SkipPlayerTurn());
            return;
        }

        if (dialogueText != null)
            dialogueText.text = "O que " + playerUnit.unitName + " fará?";

        BattleHUD.Instance.MostrarMenuPrincipal();
    }

    IEnumerator SkipPlayerTurn()
    {
        yield return new WaitForSeconds(1.5f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public void OnAttackButton()
    {
        Debug.Log("[BattleSystem] OnAttackButton state=" + state);
        if (state != BattleState.PLAYERTURN) return;

        state = BattleState.TARGETING;

        // Desativa os painéis ao clicar em atacar
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (commandsPanel != null) commandsPanel.SetActive(false);

        bool isCortante = AttackManager.Instance != null &&
                  AttackManager.Instance.armaAtual != null &&
                  AttackManager.Instance.armaAtual.tipoDeDano == TipoAtaque.Cortante;

        if (AttackManager.Instance != null && AttackManager.Instance.actionOverlayInput != null)
            AttackManager.Instance.actionOverlayInput.SetEnabled(isCortante);

        if (dialogueText != null) dialogueText.text = "Onde vai acertar?";
    }

    public void ProcessarResultadoAtaque(float multiplicador)
    {
        StartCoroutine(PlayerAttackSequence(multiplicador));
    }

    IEnumerator PlayerAttackSequence(float multiplicador)
    {
        float multDebuff = playerUnit.GetDamageMultiplierFromDebuffs();
        int danoFinal = Mathf.RoundToInt(playerUnit.strength * multiplicador * multDebuff);

        bool isDead = enemyUnit.TakeDamage(danoFinal);
        enemyHUD.UpdateHP(enemyUnit.currentHP);

        // Verifica se é batalha especial de metade de HP
        string idAtualBatalha = GameManager.Instance?.currentEnemyID ?? "Músico";
        if (!isDead
            && !string.IsNullOrEmpty(halfHpVictoryEnemyID)
            && idAtualBatalha == halfHpVictoryEnemyID
            && enemyUnit.currentHP <= enemyUnit.maxHP / 2.0f)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
            yield break;
        }

        if (dialogueText != null)
            dialogueText.text = "Ataque realizado!";

        yield return new WaitForSeconds(3f);

        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (commandsPanel != null) commandsPanel.SetActive(true);

        bool isDead = false;

        yield return new WaitForSeconds(3f);

        state = BattleState.ENEMYTURN;

        if (dialogueText != null)
            dialogueText.text = enemyUnit.unitName + " contra-ataca!";

        yield return new WaitForSeconds(3f);

        int enemyAcc = enemyUnit.accuracy;
        int playerAgiEfetiva = playerUnit.GetEffectiveAgility();

        float hitChance = HitChance.CalculateHitChance(enemyAcc, playerAgiEfetiva);
        bool hit = Random.value <= hitChance;

        if (!hit)
        {
            if (dialogueText != null)
                dialogueText.text = $"{enemyUnit.unitName} atacou, mas você desviou!";

            yield return new WaitForSeconds(2f);
            state = BattleState.PLAYERTURN;
            PlayerTurn();
            yield break;
        }

        BodyPartType part = ChooseEnemyTargetPart();

        PlayerBodyParts bodyParts = playerUnit.GetComponent<PlayerBodyParts>();
        BodyPartDefinition def = bodyParts != null ? bodyParts.Get(part) : null;

        isDead = playerUnit.TakeDamage(enemyUnit.strength);
        playerHUD.UpdateHP(playerUnit.currentHP);

        if (def != null && def.debuff != DebuffType.None)
        {
            int turns = Mathf.Max(1, def.debuffTurns);
            playerUnit.ApplyDebuff(def.debuff, turns, def.debuffStacks);
        }

        if (dialogueText != null)
        {
            if (def != null && def.debuff != DebuffType.None)
                dialogueText.text = $"{enemyUnit.unitName} acertou {PartToPtBr(part)}! ({def.debuff})";
            else
                dialogueText.text = $"{enemyUnit.unitName} acertou {PartToPtBr(part)}!";
        }

        yield return new WaitForSeconds(2f);

        if (isDead)
        {
            if (PlayerUnit.Instance != null && PlayerUnit.Instance.temForcaDeVontade)
                yield return StartCoroutine(VerificarForcaDeVontade());
            else
                StartCoroutine(GameOver());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    private IEnumerator VerificarForcaDeVontade()
    {
        bool jogadorEscolheu = false;
        bool usarForca = false;

        ForcaDeVontadeUI.Instance.Mostrar((resposta) =>
        {
            usarForca = resposta;
            jogadorEscolheu = true;
        });

        yield return new WaitUntil(() => jogadorEscolheu);

        if (usarForca)
        {
            PlayerUnit.Instance.ConsumirForcaDeVontade();
            playerUnit.currentHP = 1;
            playerHUD.UpdateHP(1);

            if (dialogueText != null)
                dialogueText.text = "Você resistiu ao golpe fatal!";

            yield return new WaitForSeconds(1.5f);
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }

    private BodyPartType ChooseEnemyTargetPart()
    {
        float r = Random.value;

        if (r < 0.10f) return BodyPartType.Head;
        if (r < 0.50f) return BodyPartType.Torso;
        if (r < 0.65f) return BodyPartType.LeftArm;
        if (r < 0.80f) return BodyPartType.RightArm;
        if (r < 0.90f) return BodyPartType.LeftLeg;
        return BodyPartType.RightLeg;
    }

    private string PartToPtBr(BodyPartType part)
    {
        switch (part)
        {
            case BodyPartType.Head: return "a cabeça";
            case BodyPartType.Torso: return "o torso";
            case BodyPartType.LeftArm: return "o braço esquerdo";
            case BodyPartType.RightArm: return "o braço direito";
            case BodyPartType.LeftLeg: return "a perna esquerda";
            case BodyPartType.RightLeg: return "a perna direita";
            default: return part.ToString();
        }
    }

    public IEnumerator EndBattle()
    {
        state = BattleState.WON;

        // Notifica o QuestManager sobre a morte do inimigo
        if (QuestManager.Instance != null && enemyUnit != null)
        {
            string enemyId = GameManager.Instance?.currentEnemyID ?? enemyUnit.unitName;
            Debug.Log($"[BattleSystem] NotificarMorteInimigo com ID: '{enemyId}'"); // ← debug tracker
            QuestManager.Instance.NotificarMorteInimigo(enemyId);
        }

        if (dialogueText != null)
            dialogueText.text = "O " + enemyUnit.unitName + " foi derrotado!";

        if (playerHUD != null && playerHUD.commandsPanel != null)
            playerHUD.commandsPanel.SetActive(false);

        yield return new WaitForSeconds(pausaAntesDeSumirInimigo);

        if (enemyHUD != null)
            yield return StartCoroutine(enemyHUD.FadeOutAndWait());
        else
            yield return StartCoroutine(FadeCanvasGroup(enemyHudCanvasGroup, 1f, 0f, duracaoFadeHudInimigo));

        yield return StartCoroutine(FadeOutEnemyTudo(duracaoFadeInimigo));

        yield return new WaitForSeconds(pausaAposFadeInimigo);

        int xpGanho = enemyUnit.expReward;
        yield return StartCoroutine(AnimarXP(xpGanho));

        yield return new WaitForSeconds(pausaAposXP);

        // ← CORRIGIDO: removido o LoadSceneWithFade duplicado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.defeatedEnemyIDs.Add(GameManager.Instance.currentEnemyID);

            string enemyIdAtual = GameManager.Instance.currentEnemyID ?? enemyUnit.unitName;
            if (GameManager.Instance.currentEnemyID == null)
                Debug.LogWarning("[BattleSystem] currentEnemyID é null; usando unitName como fallback: " + enemyUnit.unitName);
            DialogoPosBatalha entradaDialogo = null;

            if (dialogosPosVitoria != null)
            {
                Debug.Log($"[BattleSystem] enemyIdAtual='{enemyIdAtual}'");
                foreach (var entrada in dialogosPosVitoria)
                {
                    Debug.Log($"[BattleSystem] entrada.enemyId='{entrada.enemyId}'");
                    if (entrada.enemyId == enemyIdAtual && entrada.dialogo != null)
                    {
                        entradaDialogo = entrada;
                        break;
                    }
                }
            }

            if (entradaDialogo != null)
            {
                GameManager.Instance.dialogoPendente = entradaDialogo.dialogo;
                GameManager.Instance.cenaDestinoPendente = entradaDialogo.cenaAposDialogo;
                GameManager.Instance.pendingSpawnID = !string.IsNullOrEmpty(entradaDialogo.spawnIdDestino)
                    ? entradaDialogo.spawnIdDestino
                    : null;
                GameManager.Instance.isReturningFromBattle = false;

                // Carrega a cena onde o diálogo vai acontecer (ex: floresta)
                string cenaDialogo = entradaDialogo.cenaDialogo;
                if (string.IsNullOrEmpty(cenaDialogo)) cenaDialogo = nomeCenaMapa;
                GameManager.Instance.LoadSceneWithFade(cenaDialogo);
            }
            else
            {
                GameManager.Instance.isReturningFromBattle = true;
                GameManager.Instance.StartCombatGracePeriod();

                string cenaVitoria = GameManager.Instance.lastExplorationScene;
                if (string.IsNullOrEmpty(cenaVitoria)) cenaVitoria = nomeCenaMapa;
                GameManager.Instance.LoadSceneWithFade(cenaVitoria);
            }
        }
        else
        {
            Debug.LogError("[BattleSystem] GameManager.Instance é null.");
        }
    }

    IEnumerator FadeOutEnemyTudo(float duracao)
    {
        if (enemyUnit == null) yield break;

        var spriteRenderers = enemyUnit.GetComponentsInChildren<SpriteRenderer>(true);
        var images = enemyUnit.GetComponentsInChildren<UnityEngine.UI.Image>(true);

        float t = 0f;
        while (t < duracao)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / duracao);

            if (spriteRenderers != null)
                foreach (var sr in spriteRenderers)
                {
                    if (!sr) continue;
                    var c = sr.color; c.a = a; sr.color = c;
                }

            if (images != null)
                foreach (var img in images)
                {
                    if (!img) continue;
                    var c = img.color; c.a = a; img.color = c;
                }

            yield return null;
        }

        enemyUnit.gameObject.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float de, float para, float duracao)
    {
        if (cg == null) yield break;

        cg.alpha = de;
        float t = 0f;

        while (t < duracao)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(de, para, t / duracao);
            yield return null;
        }

        cg.alpha = para;
    }

    public IEnumerator AnimarXP(int xpGanho)
    {
        xpPanel.SetActive(true);

        levelText.text = "Nível " + playerUnit.playerLevel;

        xpSlider.maxValue = playerUnit.xpToNextLevel;
        xpSlider.value = playerUnit.currentXP;

        float xpVisual = playerUnit.currentXP;
        float xpAlvo = playerUnit.currentXP + xpGanho;
        float velocidadeDePreenchimento = 40f;

        while (xpVisual < xpAlvo)
        {
            xpVisual = Mathf.MoveTowards(xpVisual, xpAlvo, velocidadeDePreenchimento * Time.deltaTime);
            xpSlider.value = xpVisual;

            if (xpSlider.value >= xpSlider.maxValue)
            {
                playerUnit.playerLevel++;
                levelText.text = "Subiu de nível!";

                xpAlvo -= xpSlider.maxValue;
                xpVisual = 0;
                xpSlider.value = 0;

                playerUnit.xpToNextLevel = Mathf.RoundToInt(playerUnit.xpToNextLevel * 1.5f);
                xpSlider.maxValue = playerUnit.xpToNextLevel;

                yield return new WaitForSeconds(1f);
            }

            yield return null;
        }

        playerUnit.currentXP = Mathf.RoundToInt(xpVisual);
    }

    public void OnFugirButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        string idAtualBatalha = GameManager.Instance?.currentEnemyID ?? "";
        if (!string.IsNullOrEmpty(noFleeEnemyID) && idAtualBatalha == noFleeEnemyID)
        {
            if (dialogueText != null)
                dialogueText.text = "Você não pode fugir desta batalha!";
            return;
        }

        StartCoroutine(TentarFugir());
    }

    private IEnumerator TentarFugir()
    {
        state = BattleState.BUSY;

        bool falhou = Random.value < 0.2f;

        if (falhou)
        {
            if (dialogueText != null)
                dialogueText.text = "Não foi possível fugir!";

            yield return new WaitForSeconds(2f);

            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = "Você fugiu da batalha!";

            yield return new WaitForSeconds(1.5f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.isReturningFromBattle = true;
                GameManager.Instance.StartCombatGracePeriod();
                GameManager.Instance.LoadSceneWithFade(nomeCenaMapa);

                string cenaFuga = GameManager.Instance.lastExplorationScene;
                if (string.IsNullOrEmpty(cenaFuga)) cenaFuga = nomeCenaMapa;
                GameManager.Instance.LoadSceneWithFade(cenaFuga);
            }
        }
    }

    public void PassarTurnoAposItem()
    {
        if (state != BattleState.PLAYERTURN) return;

        if (dialogueText != null)
            dialogueText.text = playerUnit.unitName + " usou um item!";

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator GameOver()
    {
        state = BattleState.LOST;

        if (playerHUD != null && playerHUD.commandsPanel != null)
            playerHUD.commandsPanel.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        if (GameOverPanelUI.Instance != null)
            GameOverPanelUI.Instance.Mostrar();
        else
            Debug.LogError("[BattleSystem] GameOverPanelUI.Instance não encontrado na cena!");
    }
}