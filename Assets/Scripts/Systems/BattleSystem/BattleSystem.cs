using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    [Header("Configuração")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab; // Prefab padrão (ex: Javali)

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    [Header("Unidades")]
    Unit playerUnit;
    Unit enemyUnit;
    GameObject enemyGO;

    [Header("UI")]
    public Button attackButton;
    public Button defendButton;
    public Button magicButton;
    public GameObject magicMenuPanel;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    public BattleLogManager battleLogManager;
    public TurnIndicator turnIndicator;

    [Header("UI de Fim de Jogo")]
    public GameObject levelUpPanel;
    public TextMeshProUGUI levelUpStatsText;
    public GameObject gameOverPanel;
    // (VictoryPanel removido pois agora é na Exploração)

    [Header("UI de XP")]
    public GameObject xpDisplayPanel;
    public Slider xpSlider;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;

    void Start()
    {
        // Segurança: Garante que painéis comecem fechados
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (xpDisplayPanel != null) xpDisplayPanel.SetActive(false);

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // 1. Limpa a estação de batalha (remove inimigos de teste)
        foreach (Transform child in enemyBattleStation)
        {
            Destroy(child.gameObject);
        }

        // 2. Cria o Jogador
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation.position, Quaternion.identity);
        playerUnit = playerGO.GetComponent<PlayerUnit>();
        playerUnit.InicializarUnidade();
        playerGO.transform.parent = playerBattleStation;

        // 3. DECIDE QUAL INIMIGO SPAWNAR
        GameObject prefabToSpawn = enemyPrefab; // Começa com o padrão

        // Se o GameManager tiver um inimigo específico (o que veio da exploração)
        if (GameManager.instance.nextBattleEnemyPrefab != null)
        {
            prefabToSpawn = GameManager.instance.nextBattleEnemyPrefab;
        }

        // 4. Cria o Inimigo Certo
        enemyGO = Instantiate(prefabToSpawn, enemyBattleStation.position, Quaternion.identity);
        enemyUnit = enemyGO.GetComponent<EnemyUnit>();
        enemyGO.transform.parent = enemyBattleStation;
        enemyUnit.InicializarUnidade();

        // 5. Configura a UI
        battleLogManager.AddLogMessage("Um " + enemyUnit.unitName + " selvagem apareceu!");

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        playerHUD.UpdateMP(playerUnit.currentMP);

        turnIndicator.Hide();

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        battleLogManager.AddLogMessage("É a sua vez! Escolha uma ação.");
        SetActionButtons(true);
        playerUnit.isDefending = false;
        turnIndicator.SetTarget(playerUnit.transform, true);
    }

    void SetActionButtons(bool isActive)
    {
        attackButton.interactable = isActive;
        defendButton.interactable = isActive;
        magicButton.interactable = isActive;
    }

    // --- BOTÕES ---

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(PlayerAttack());
    }

    public void OnDefendButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(PlayerDefend());
    }

    public void OnMagicButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        SetActionButtons(false);
        magicMenuPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        magicMenuPanel.SetActive(false);
        SetActionButtons(true);
    }

    public void OnFireballButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        magicMenuPanel.SetActive(false);
        StartCoroutine(PlayerMagicAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        magicMenuPanel.SetActive(false);
        StartCoroutine(PlayerHeal());
    }

    // --- AÇÕES ---

    IEnumerator PlayerAttack()
    {
        SetActionButtons(false); // Trava botões

        // Cálculo de Dano Físico
        int rawDamage = playerUnit.strength;
        bool isCritical = false;
        if (Random.Range(1, 101) <= playerUnit.luck)
        {
            isCritical = true;
            rawDamage *= 2;
        }

        // Calcula dano real (Ataque - Defesa)
        int defense = enemyUnit.resistance;
        int finalDamage = rawDamage - defense;
        if (finalDamage < 1) finalDamage = 1;

        bool isDead = enemyUnit.TakeDamage(finalDamage); // Unit.cs só subtrai

        enemyHUD.UpdateHP(enemyUnit.currentHP);

        if (isCritical) battleLogManager.AddLogMessage($"CRÍTICO! Causou {finalDamage} de dano!");
        else battleLogManager.AddLogMessage($"Causou {finalDamage} de dano!");

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(enemyUnit.FadeOut());
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerMagicAttack()
    {
        int manaCost = 20;
        if (playerUnit.currentMP < manaCost)
        {
            battleLogManager.AddLogMessage("Mana insuficiente!");
            SetActionButtons(true);
            yield break;
        }

        SetActionButtons(false);
        playerUnit.currentMP -= manaCost;
        playerHUD.UpdateMP(playerUnit.currentMP);

        // Cálculo de Dano Mágico
        int rawDamage = playerUnit.will;
        int defense = enemyUnit.knowledge;
        int finalDamage = rawDamage - defense;
        if (finalDamage < 1) finalDamage = 1;

        battleLogManager.AddLogMessage($"{playerUnit.unitName} usa Bola de Fogo!");
        yield return new WaitForSeconds(1f);

        bool isDead = enemyUnit.TakeDamage(finalDamage);
        enemyHUD.UpdateHP(enemyUnit.currentHP);

        battleLogManager.AddLogMessage($"Bola de Fogo causou {finalDamage} de dano!");
        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(enemyUnit.FadeOut());
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerDefend()
    {
        SetActionButtons(false);
        playerUnit.isDefending = true;
        battleLogManager.AddLogMessage("Defendendo!");
        yield return new WaitForSeconds(1f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerHeal()
    {
        int manaCost = 15;
        if (playerUnit.currentMP < manaCost)
        {
            battleLogManager.AddLogMessage("Mana insuficiente!");
            SetActionButtons(true);
            yield break;
        }
        playerUnit.currentMP -= manaCost;
        playerHUD.UpdateMP(playerUnit.currentMP);

        int heal = playerUnit.will + 10;
        playerUnit.Heal(heal);
        playerHUD.UpdateHP(playerUnit.currentHP);

        battleLogManager.AddLogMessage($"Curou {heal} HP!");
        yield return new WaitForSeconds(1f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    // --- TURNO DO INIMIGO ---

    IEnumerator EnemyTurn()
    {
        turnIndicator.SetTarget(enemyUnit.transform, false);

        battleLogManager.AddLogMessage($"{enemyUnit.unitName} ataca!");
        yield return new WaitForSeconds(1f);

        // Cálculo de Dano do Inimigo
        int rawDamage = enemyUnit.strength;
        bool isCritical = false;
        if (Random.Range(1, 101) <= enemyUnit.luck)
        {
            isCritical = true;
            rawDamage *= 2;
        }

        // Defesa do Jogador
        int playerDef = playerUnit.resistance;
        if (playerUnit.isDefending) playerDef *= 2; // Bônus de defesa

        int finalDamage = rawDamage - playerDef;
        if (finalDamage < 1) finalDamage = 1;

        bool playerDead = playerUnit.TakeDamage(finalDamage);
        playerHUD.UpdateHP(playerUnit.currentHP);

        if (isCritical) battleLogManager.AddLogMessage($"CRÍTICO! Recebeu {finalDamage} de dano!");
        else battleLogManager.AddLogMessage($"Recebeu {finalDamage} de dano.");

        yield return new WaitForSeconds(1f);

        if (playerDead)
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

    // --- FIM DA BATALHA ---

    IEnumerator EndBattle()
    {
        turnIndicator.Hide();

        if (state == BattleState.WON)
        {
            battleLogManager.AddLogMessage("Vitória!");

            // 1. Salva HP/MP atuais
            GameManager.instance.currentHP = playerUnit.currentHP;
            GameManager.instance.currentMP = playerUnit.currentMP;

            // 2. PREPARAÇÃO DOS DADOS PARA ANIMAÇÃO
            // Pegamos os valores ANTIGOS (como o jogador estava antes de ganhar o XP)
            // Precisamos acessar o GameManager diretamente antes de atualizá-lo
            int xpInicial = GameManager.instance.currentXP;
            int xpMaxInicial = GameManager.instance.xpToNextLevel;
            int nivelInicial = GameManager.instance.playerLevel;

            // 3. Aplica o XP e Sobe de Nível (se necessário) no Back-end
            EnemyUnit enemy = enemyUnit as EnemyUnit;

            int xpGanho = (enemy != null) ? enemy.xpValue : 0;
            GameManager.instance.GainXP(xpGanho);

            // 3.1. Lógica de drop de Itens
            if (enemy != null && enemy.tabelaDeLoot != null)
            {
                foreach (Loot drop in enemy.tabelaDeLoot)
                {
                    float sorteio = Random.Range(0f, 100f);

                    if (sorteio <= drop.chanceDeDrop)
                    {
                        InventoryManager.Instance.AdicionarItem(drop.item, drop.quantidade);
                        battleLogManager.AddLogMessage($"Item obtido: {drop.quantidade}x {drop.item.nomeItem}");
                    }
                }
            }

            // 4. Pega os valores NOVOS (como ficou depois)
            int xpFinal = GameManager.instance.currentXP;
            int xpMaxFinal = GameManager.instance.xpToNextLevel;
            int nivelFinal = GameManager.instance.playerLevel;

            // 5. LÓGICA DA BARRA DE XP (VISUAL)
            if (xpDisplayPanel != null)
            {
                xpDisplayPanel.SetActive(true);
                xpText.text = $"XP Ganho: {xpGanho}";
                levelText.text = "Nível " + nivelInicial;

                // Começa a barra na posição antiga
                xpSlider.maxValue = xpMaxInicial;
                xpSlider.value = xpInicial;

                float timer = 0f;
                float duration = 1.0f; // A animação leva 1 segundo

                // CASO 1: HOUVE LEVEL UP
                if (nivelFinal > nivelInicial)
                {
                    // Parte A: Enche a barra até o topo
                    while (timer < duration)
                    {
                        timer += Time.deltaTime;
                        xpSlider.value = Mathf.Lerp(xpInicial, xpMaxInicial, timer / duration);
                        yield return null;
                    }
                    xpSlider.value = xpMaxInicial;

                    // Som/Efeito de Level Up
                    battleLogManager.AddLogMessage($"LEVEL UP! Nível {nivelFinal}!");
                    levelText.text = "Nível " + nivelFinal;

                    // Parte B: Reseta a barra para o novo nível
                    xpSlider.value = 0;
                    xpSlider.maxValue = xpMaxFinal;

                    // Ajusta variáveis para a próxima animação
                    xpInicial = 0;
                    timer = 0f;
                }

                // CASO 2: PREENCHE O RESTANTE (Ou o normal se não houve level up)
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    xpSlider.value = Mathf.Lerp(xpInicial, xpFinal, timer / duration);
                    yield return null;
                }
                xpSlider.value = xpFinal; // Garante o valor exato no final
            }

            // 6. Finalização (Save e Load)
            GameManager.instance.defeatedEnemyIDs.Add(GameManager.instance.currentEnemyID);

            yield return new WaitForSeconds(2.5f); // Tempo para ler o painel

            if (GameManager.instance.isBossBattle)
            {
                GameManager.instance.triggerEndingOnLoad = true;
                GameManager.instance.isBossBattle = false;
            }

            string sceneToLoad = GameManager.instance.lastExplorationScene;
            if (string.IsNullOrEmpty(sceneToLoad)) sceneToLoad = "MenuPrincipalScene";

            GameManager.instance.LoadSceneWithFade(sceneToLoad);
        }
        else if (state == BattleState.LOST)
        {
            battleLogManager.AddLogMessage("Derrota...");
            yield return new WaitForSeconds(2f);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }
}