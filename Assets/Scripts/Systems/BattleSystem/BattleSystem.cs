using UnityEngine;
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

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
    public PlayerUnit playerUnit; // setado em runtime via PlayerUnit.Instance

    [Header("Interface (HUD)")]
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    public TextMeshProUGUI dialogueText;

    [Header("HUD do Inimigo (Fade rápido)")]
    public CanvasGroup enemyHudCanvasGroup;

    [Header("Durações")]
    public float duracaoFadeInimigo = 0.6f;
    public float duracaoFadeHudInimigo = 0.25f;

    [Header("Pausas entre etapas")]
    public float pausaAntesDeSumirInimigo = 0.4f;
    public float pausaAposFadeInimigo = 0.35f;
    public float pausaAposXP = 0.9f;

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
        // Sempre usa o PlayerUnit persistente
        if (PlayerUnit.Instance == null)
        {
            Debug.LogError("[BattleSystem] PlayerUnit.Instance é NULL. Garanta que existe um PlayerUnit persistente antes da BattleScene carregar.");
            yield break;
        }

        playerUnit = PlayerUnit.Instance;
        playerUnit.InicializarUnidade();

        // Spawna inimigo
        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyGO.transform.localPosition = Vector3.zero;
        enemyUnit = enemyGO.GetComponent<EnemyUnit>();

        if (enemyUnit == null)
        {
            Debug.LogError("[BattleSystem] enemyPrefab não tem componente EnemyUnit.");
            yield break;
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

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        if (dialogueText != null)
            dialogueText.text = "O que " + playerUnit.unitName + " fará?";

        BattleHUD.Instance.MostrarMenuPrincipal();
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        state = BattleState.BUSY;

        if (dialogueText != null) dialogueText.text = "Onde vai acertar?";
    }

    public void ProcessarResultadoAtaque(float multiplicador)
    {
        StartCoroutine(PlayerAttackSequence(multiplicador));
    }

    IEnumerator PlayerAttackSequence(float multiplicador)
    {
        int danoFinal = Mathf.RoundToInt(playerUnit.strength * multiplicador);
        bool isDead = enemyUnit.TakeDamage(danoFinal);
        enemyHUD.UpdateHP(enemyUnit.currentHP);

        if (dialogueText != null)
            dialogueText.text = "Ataque realizado!";

        yield return new WaitForSeconds(2f);

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
        yield return new WaitForSeconds(1.5f);

        state = BattleState.ENEMYTURN;

        if (dialogueText != null)
            dialogueText.text = enemyUnit.unitName + " contra-ataca!";

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.strength);
        playerHUD.UpdateHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    public IEnumerator EndBattle()
    {
        state = BattleState.WON;

        if (dialogueText != null)
            dialogueText.text = "O " + enemyUnit.unitName + " foi derrotado!";

        if (playerHUD != null && playerHUD.commandsPanel != null)
            playerHUD.commandsPanel.SetActive(false);

        yield return new WaitForSeconds(pausaAntesDeSumirInimigo);

        // barra do inimigo some primeiro
        if (enemyHUD != null)
            yield return StartCoroutine(enemyHUD.FadeOutAndWait());
        else
            yield return StartCoroutine(FadeCanvasGroup(enemyHudCanvasGroup, 1f, 0f, duracaoFadeHudInimigo));

        // depois o inimigo some
        yield return StartCoroutine(FadeOutEnemyTudo(duracaoFadeInimigo));

        yield return new WaitForSeconds(pausaAposFadeInimigo);

        // XP
        int xpGanho = enemyUnit.expReward;
        yield return StartCoroutine(AnimarXP(xpGanho));

        yield return new WaitForSeconds(pausaAposXP);

        if (GameManager.Instance != null) GameManager.Instance.LoadSceneWithFade(nomeCenaMapa);
        else Debug.LogError("GameManager.Instance é null. Não foi possível fazer LoadSceneWithFade.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.defeatedEnemyIDs.Add(GameManager.Instance.currentEnemyID);
            GameManager.Instance.isReturningFromBattle = true; // garante a flag
            GameManager.Instance.StartCombatGracePeriod();
            GameManager.Instance.LoadSceneWithFade(nomeCenaMapa);
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
            {
                foreach (var sr in spriteRenderers)
                {
                    if (!sr) continue;
                    var c = sr.color;
                    c.a = a;
                    sr.color = c;
                }
            }

            if (images != null)
            {
                foreach (var img in images)
                {
                    if (!img) continue;
                    var c = img.color;
                    c.a = a;
                    img.color = c;
                }
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

                yield return new WaitForSeconds(0.8f);
            }

            yield return null;
        }

        playerUnit.currentXP = Mathf.RoundToInt(xpVisual);

        // Se você quer persistir isso no GameManager também, aqui é o ponto:
    }
}