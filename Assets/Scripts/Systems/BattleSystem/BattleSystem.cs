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
    // Arraste aqui um objeto que tenha o script Unit com os status do jogador
    public PlayerUnit playerUnit;

    [Header("Interface (HUD)")]
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    public TextMeshProUGUI dialogueText;

    [Header("Transição")]
    public UnityEngine.UI.Image telaDeFade;

    public BattleState state;

    private void Awake()
    {
        if (Instance == null) Instance = this; // Define a instância ao carregar a cena
    }


    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // --- PARTE 1: O INIMIGO ---
        // Instancia o prefab do monstro na cena
        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyGO.transform.localPosition = Vector3.zero;
        enemyUnit = enemyGO.GetComponent<EnemyUnit>();

        // --- PARTE 2: O JOGADOR (DADOS DO GAME MANAGER) ---
        // Buscamos os valores reais do seu GameManager para o Humanitis
        if (GameManager.Instance != null && playerUnit != null)
        {
            playerUnit.unitName = "Caçador"; // Ou pegue do Manager se preferir
            playerUnit.maxHP = GameManager.Instance.maxHP;
            playerUnit.currentHP = GameManager.Instance.currentHP;

            // O dano pode ser a força base do manager + bônus de arma
            playerUnit.strength = GameManager.Instance.strength;
        }
        else
        {
            Debug.LogWarning("GameManager não encontrado! Usando valores do Inspector para teste.");
        }

        // --- PARTE 3: A INTERFACE (HUD) ---
        // O diálogo inicial usa o nome do monstro
        dialogueText.text = "Um " + enemyUnit.unitName + " bloqueia seu caminho...";

        // Configuramos o HUD do Jogador (que agora carrega o Portrait)
        if (playerHUD != null)
        {
            playerHUD.SetHUD(playerUnit);
        }

        // Configuramos o HUD do Inimigo
        if (enemyHUD != null)
        {
            enemyHUD.SetHUD(enemyUnit);
        }

        // --- PARTE 4: TRANSIÇÃO ---
        // Espera 2 segundos para o jogador ler o diálogo e ver o monstro
        yield return new WaitForSeconds(2f);

        if (AttackManager.Instance == null)
        {
            Debug.LogError("ERRO: O objeto AttackManager não foi encontrado na cena!");
            yield break;
        }

        if (EquipmentManager.Instance == null) // Ou GameManager, dependendo de onde está sua arma
        {
            Debug.LogError("ERRO: O EquipmentManager não foi encontrado na cena!");
            yield break;
        }

        if (AttackManager.Instance != null && EquipmentManager.Instance != null)
        {
            // Pegamos o que está no inventário e passamos para o cérebro do ataque
            AttackManager.Instance.armaAtual = (DadosArma)EquipmentManager.Instance.currentEquipment[0];

            // Log para você confirmar no Console se a arma chegou
            Debug.Log("Sucesso: AttackManager recebeu a arma " + AttackManager.Instance.armaAtual.name);
        }
        else
        {
            Debug.LogError("Falha Crítica: AttackManager ou EquipmentManager não encontrados na cena!");
        }

        // Muda para o turno do jogador e libera o menu de comandos
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
            EndBattle();
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

        // O dano vai direto para o playerUnit que está no HUD
        bool isDead = playerUnit.TakeDamage(enemyUnit.strength);
        playerHUD.UpdateHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    public IEnumerator EndBattle()
    {
        // 1. Muda o estado para evitar cliques extras
        state = BattleState.WON;
        dialogueText.text = "O " + enemyUnit.unitName + " foi derrotado!";

        // Esconde o painel de comandos para limpar a tela
        if (playerHUD.commandsPanel != null)
            playerHUD.commandsPanel.SetActive(false);

        // 2. Aguarda um momento dramático e faz o Fade Out do Inimigo
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeOutEnemy());

        int xpGanho = enemyUnit.expReward;
        StartCoroutine(AnimarXP(xpGanho));

        yield return new WaitForSeconds(2f);
        if (telaDeFade != null)
        {
            telaDeFade.gameObject.SetActive(true); // Liga a tela preta
            Color cor = telaDeFade.color;
            cor.a = 0f; // Começa 100% transparente
            telaDeFade.color = cor;

            float tempoFade = 1f; // Duração do escurecimento (1 segundo)
            float tempoAtual = 0f;

            while (tempoAtual < tempoFade)
            {
                tempoAtual += Time.deltaTime;
                cor.a = Mathf.Lerp(0f, 1f, tempoAtual / tempoFade); // Vai escurecendo
                telaDeFade.color = cor;
                yield return null; // Espera o próximo frame
            }
        }

        // Só depois da tela ficar totalmente preta, carrega o mapa
        Debug.Log("Retornando para a cena: " + nomeCenaMapa);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCenaMapa);
    }

    // Lógica para sumir com o monstro gradativamente
    IEnumerator FadeOutEnemy()
    {
        // Tenta pegar o SpriteRenderer se o monstro for um objeto 2D no mundo
        SpriteRenderer sr = enemyUnit.GetComponentInChildren<SpriteRenderer>();

        // Tenta pegar a Image se o monstro for um elemento de UI no Canvas
        UnityEngine.UI.Image img = enemyUnit.GetComponentInChildren<UnityEngine.UI.Image>();

        float velocidadeFade = 1.5f;

        if (sr != null)
        {
            Color cor = sr.color;
            while (cor.a > 0)
            {
                cor.a -= Time.deltaTime * velocidadeFade;
                sr.color = cor;
                yield return null;
            }
        }
        else if (img != null)
        {
            Color cor = img.color;
            while (cor.a > 0)
            {
                cor.a -= Time.deltaTime * velocidadeFade;
                img.color = cor;
                yield return null;
            }
        }

        // Desativa o objeto do monstro completamente ao terminar de sumir
        enemyUnit.gameObject.SetActive(false);
    }

    public IEnumerator AnimarXP(int xpGanho)
    {
        // 1. Liga o painel e define o texto inicial
        xpPanel.SetActive(true);
        levelText.text = "Nível " + playerUnit.unitLevel;

        // 2. Configura a barra com o XP que o jogador já tinha antes da luta
        xpSlider.maxValue = playerUnit.maxXP; // O total necessário para upar
        xpSlider.value = playerUnit.currentXP;

        float xpVisual = playerUnit.currentXP;
        float xpAlvo = playerUnit.currentXP + xpGanho;
        float velocidadeDePreenchimento = 40f; // Ajuste para a barra encher mais rápido/devagar

        // 3. Loop de animação: roda até o XP visual alcançar o alvo
        while (xpVisual < xpAlvo)
        {
            // Move o valor visualmente frame a frame
            xpVisual = Mathf.MoveTowards(xpVisual, xpAlvo, velocidadeDePreenchimento * Time.deltaTime);
            xpSlider.value = xpVisual;

            // 4. LÓGICA DE LEVEL UP: Se a barra bater no limite
            if (xpSlider.value >= xpSlider.maxValue)
            {
                playerUnit.unitLevel++;
                levelText.text = "Subiu de nível!"; // Substitui o texto inteiro

                // Subtrai o XP que foi "gasto" para upar e zera a barra para o resto do XP
                xpAlvo -= xpSlider.maxValue;
                xpVisual = 0;
                xpSlider.value = 0;

                // Define o novo limite de XP para o próximo nível (Ex: aumenta 50% a dificuldade)
                playerUnit.maxXP = Mathf.RoundToInt(playerUnit.maxXP * 1.5f);
                xpSlider.maxValue = playerUnit.maxXP;

                // Pausa dramática para o jogador comemorar o level up antes da barra voltar a encher
                yield return new WaitForSeconds(0.8f);
            }

            yield return null;
        }

        // 5. Salva o XP que sobrou na variável oficial do jogador após a animação acabar
        playerUnit.currentXP = Mathf.RoundToInt(xpVisual);
    }
}