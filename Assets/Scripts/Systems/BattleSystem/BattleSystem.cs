using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para trocar de cena
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    [Header("Telas de Vitória")]
    public GameObject xpPanel;
    public UnityEngine.UI.Slider xpSlider; // O Slider que você criou na imagem
    public TextMeshProUGUI levelText;      // O texto "Nível X"

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

    [Header("Transição (Fade da Tela)")]
    public UnityEngine.UI.Image telaDeFade;

    [Header("HUD do Inimigo (Fade rápido)")]
    [Tooltip("Coloque aqui o CanvasGroup do HUD do inimigo (barra de vida etc.).")]
    public CanvasGroup enemyHudCanvasGroup;

    [Header("Durações")]
    [Tooltip("Duração do fade do inimigo (corpo + ponto fraco + filhos).")]
    public float duracaoFadeInimigo = 0.6f;

    [Tooltip("Duração do fade do HUD do inimigo (mais rápido que o inimigo).")]
    public float duracaoFadeHudInimigo = 0.25f;

    [Tooltip("Duração do fade-out da tela antes de trocar de cena.")]
    public float duracaoFadeTela = 0.5f;

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

        // Deixa o fade de tela pronto (ativo e transparente) para evitar "não apareceu"
        if (telaDeFade != null)
        {
            telaDeFade.gameObject.SetActive(true);
            var c = telaDeFade.color;
            c.a = 0f;
            telaDeFade.color = c;
        }
    }

    IEnumerator SetupBattle()
    {
        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyGO.transform.localPosition = Vector3.zero;
        enemyUnit = enemyGO.GetComponent<EnemyUnit>();

        if (GameManager.Instance != null && playerUnit != null)
        {
            playerUnit.unitName = "Caçador";
            playerUnit.maxHP = GameManager.Instance.maxHP;
            playerUnit.currentHP = GameManager.Instance.currentHP;
            playerUnit.strength = GameManager.Instance.strength;
        }
        else
        {
            Debug.LogWarning("GameManager não encontrado! Usando valores do Inspector para teste.");
        }

        dialogueText.text = "Um " + enemyUnit.unitName + " bloqueia seu caminho...";

        if (playerHUD != null) playerHUD.SetHUD(playerUnit);
        if (enemyHUD != null) enemyHUD.SetHUD(enemyUnit);

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
        else
        {
            Debug.LogError("Falha Crítica: AttackManager ou EquipmentManager não encontrados na cena!");
        }

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        dialogueText.text = "O que " + playerUnit.unitName + " fará?";
        BattleHUD.Instance.MostrarMenuPrincipal();
    }

    public void OnAttackButton()
    {
        Debug.Log("PASSO 1: Botão Atacar clicado. Mudando estado para BUSY.");

        if (state != BattleState.PLAYERTURN) return;

        state = BattleState.BUSY;

        Debug.Log("Onde vai acertar?");
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
        dialogueText.text = "O " + enemyUnit.unitName + " foi derrotado!";

        if (playerHUD != null && playerHUD.commandsPanel != null)
            playerHUD.commandsPanel.SetActive(false);

        // 1) inimigo
        yield return new WaitForSeconds(pausaAntesDeSumirInimigo);

        // Fade do HUD do inimigo MAIS RÁPIDO (barra some primeiro)
        yield return StartCoroutine(FadeCanvasGroup(enemyHudCanvasGroup, 1f, 0f, duracaoFadeHudInimigo));

        // Fade do inimigo (corpo + weakpoint + filhos)
        yield return StartCoroutine(FadeOutEnemyTudo(duracaoFadeInimigo));

        yield return new WaitForSeconds(pausaAposFadeInimigo);

        // 2) XP (espera terminar!)
        int xpGanho = enemyUnit.expReward;
        yield return StartCoroutine(AnimarXP(xpGanho));

        yield return new WaitForSeconds(pausaAposXP);

        // 3) transição
        yield return StartCoroutine(FadeTela(0f, 1f, duracaoFadeTela));

        Debug.Log("Retornando para a cena: " + nomeCenaMapa);
        SceneManager.LoadScene(nomeCenaMapa);
    }

    IEnumerator FadeOutEnemyTudo(float duracao)
    {
        if (enemyUnit == null)
            yield break;

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

    IEnumerator FadeTela(float de, float para, float duracao)
    {
        if (telaDeFade == null) yield break;

        telaDeFade.gameObject.SetActive(true);

        Color cor = telaDeFade.color;
        cor.a = de;
        telaDeFade.color = cor;

        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime; // mais robusto (se usar timeScale=0 em algum momento)
            cor.a = Mathf.Lerp(de, para, t / duracao);
            telaDeFade.color = cor;
            yield return null;
        }

        cor.a = para;
        telaDeFade.color = cor;
    }

    public IEnumerator AnimarXP(int xpGanho)
    {
        xpPanel.SetActive(true);
        levelText.text = "Nível " + playerUnit.unitLevel;

        xpSlider.maxValue = playerUnit.maxXP;
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
                playerUnit.unitLevel++;
                levelText.text = "Subiu de nível!";

                xpAlvo -= xpSlider.maxValue;
                xpVisual = 0;
                xpSlider.value = 0;

                playerUnit.maxXP = Mathf.RoundToInt(playerUnit.maxXP * 1.5f);
                xpSlider.maxValue = playerUnit.maxXP;

                yield return new WaitForSeconds(0.8f);
            }

            yield return null;
        }

        playerUnit.currentXP = Mathf.RoundToInt(xpVisual);

        // Se quiser manter a tela de XP visível um tempinho extra, aumente aqui:
        // yield return new WaitForSeconds(0.8f);
    }
}