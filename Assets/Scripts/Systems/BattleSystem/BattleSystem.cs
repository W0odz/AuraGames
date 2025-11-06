using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Necessário para Coroutines

// Enum para definir os estados da batalha
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

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

    [Header("Indicadores")]
    public TurnIndicator turnIndicator; // Arraste o TurnIndicator aqui

    [Header("UI de XP")]
    public GameObject xpDisplayPanel; // Arraste o XP_Display_Panel aqui
    public Slider xpSlider;           // Arraste o XP_Slider aqui
    public TextMeshProUGUI xpText;    // Arraste o XP_Text aqui
    public TextMeshProUGUI levelText; // Arraste o Level_Text aqui

    #region Métodos Unity
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }
    #endregion

    #region Sistema de Batalha
    void SetActionButtons(bool isActive)
    {
        attackButton.interactable = isActive;
        defendButton.interactable = isActive;
        magicButton.interactable = isActive;
    }

    // Corrotina para configurar a batalha
    IEnumerator SetupBattle()
    {
        // Instancia o jogador e o inimigo na cena
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation.position, Quaternion.identity);
        playerUnit = playerGO.GetComponent<Unit>();
        // Puxa todos os stats do GameManager!
        playerUnit.SetupPlayerStats(GameManager.instance);
        playerGO.transform.parent = playerBattleStation;

        enemyGO = Instantiate(enemyPrefab, enemyBattleStation.position, Quaternion.identity);
        enemyUnit = enemyGO.GetComponent<Unit>();
        enemyGO.transform.parent = enemyBattleStation;

        battleLogManager.AddLogMessage("Um " + enemyUnit.unitName + " selvagem apareceu!");

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        playerHUD.UpdateMP(playerUnit.currentMP);

        turnIndicator.Hide();

        // Espera 2 segundos antes de começar o turno do jogador
        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        battleLogManager.AddLogMessage("É a sua vez! Escolha uma ação.");
        attackButton.interactable = true; // Habilita o botão de ataque
        SetActionButtons(true); // Habilita todos os botões

        turnIndicator.SetTarget(playerUnit.transform);
    }


    #region Botões do jogador
    // Esta função será chamada pelo botão de ataque
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }
    public void OnDefendButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerDefend());
    }

    public void OnMagicButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Esconde os botões de ação principais e mostra o menu de magias
        SetActionButtons(false);
        magicMenuPanel.SetActive(true);
    }

    public void OnFireballButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Esconde o menu de magias
        magicMenuPanel.SetActive(false);

        // A lógica do ataque mágico agora fica aqui
        StartCoroutine(PlayerMagicAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Esconde o menu de magias
        magicMenuPanel.SetActive(false);

        StartCoroutine(PlayerHeal());
    }

    public void OnBackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Esconde o menu de magias e mostra os botões de ação principais
        magicMenuPanel.SetActive(false);
        SetActionButtons(true);
    }
    #endregion

    #region Corrotinas dos turnos
    #region Ações do jogador
    IEnumerator PlayerAttack()
    {
        // Desabilita o botão para evitar múltiplos cliques
        attackButton.interactable = false;
        SetActionButtons(false); // Desabilita todos os botões

        // Causa dano no inimigo
        bool isCritical = false;
        int damage = playerUnit.strength;
        bool isDead = enemyUnit.TakeDamage(damage, false); // false = não é mágico

        int critRoll = Random.Range(1, 101);
        if (critRoll <= playerUnit.luck) // Se o resultado for menor ou igual à Sorte
        {
            isCritical = true;
            damage *= 2; // Dobra o dano
        }

        enemyHUD.UpdateHP(enemyUnit.currentHP);

        if (isCritical)
        {
            battleLogManager.AddLogMessage("ACERTO CRÍTICO! " + playerUnit.unitName + " ataca e causa " + damage + " de dano!");
        }
        else
        {
            battleLogManager.AddLogMessage(playerUnit.unitName + " ataca e causa " + damage + " de dano!");
        }

        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(enemyUnit.FadeOut()); // Deixa o fade do inimigo terminar
            yield return new WaitForSeconds(1.5f); // Espera o fade do inimigo
            StartCoroutine(EndBattle()); // Chama o fim
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

        //Dano da magia é baseado na Vontade (Will)
        int magicDamage = playerUnit.will + 10;
        battleLogManager.AddLogMessage($"{playerUnit.unitName} usa uma bola de fogo!");
        yield return new WaitForSeconds(1f);

        bool isDead = enemyUnit.TakeDamage(magicDamage, true); // true = é mágico
        enemyHUD.UpdateHP(enemyUnit.currentHP);
        battleLogManager.AddLogMessage($"O ataque causa {magicDamage} de dano!");
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
        // Desabilita todos os botões
        SetActionButtons(false);

        playerUnit.isDefending = true;

        battleLogManager.AddLogMessage($"{playerUnit.unitName} se prepara para defender!");

        yield return new WaitForSeconds(1f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerHeal()
    {
        int manaCost = 15;

        // 1. Verifica se tem mana suficiente
        if (playerUnit.currentMP < manaCost)
        {
            battleLogManager.AddLogMessage("Mana insuficiente!");
            // Mostra os botões de ação novamente para o jogador escolher outra coisa
            SetActionButtons(true);
            yield break;
        }

        // 2. Executa a cura
        playerUnit.currentMP -= manaCost;
        playerHUD.UpdateMP(playerUnit.currentMP);

        int healAmount = playerUnit.will + 10;

        playerUnit.Heal(healAmount);
        playerHUD.UpdateHP(playerUnit.currentHP);

        battleLogManager.AddLogMessage($"{playerUnit.unitName} se cura em {healAmount} pontos de vida!");
        yield return new WaitForSeconds(1f);

        // 3. Passa o turno para o inimigo
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }
    #endregion
    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        turnIndicator.SetTarget(enemyUnit.transform);
        battleLogManager.AddLogMessage($"{enemyUnit.unitName} ataca!");
        yield return new WaitForSeconds(1f);

        bool isCritical = false;
        int damage = enemyUnit.strength; // Pega a força base do inimigo

        // Rola um dado de 100 lados
        int critRoll = Random.Range(1, 101);
        if (critRoll <= enemyUnit.luck) // Usa a Sorte do inimigo
        {
            isCritical = true;
            damage *= 2; // Dobra o dano
        }
        bool isDead = playerUnit.TakeDamage(damage, false);

        playerHUD.UpdateHP(playerUnit.currentHP);

        if (isCritical)
        {
            battleLogManager.AddLogMessage("ACERTO CRÍTICO! " + enemyUnit.unitName + " ataca e causa " + damage + " de dano!");
        }
        else
        {
            battleLogManager.AddLogMessage("O ataque causa " + damage + " de dano!");
        }

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
    #endregion

    IEnumerator EndBattle()
    {
        turnIndicator.Hide();

        if (state == BattleState.WON)
        {
            battleLogManager.AddLogMessage("Você venceu a batalha!");

            // 1. Salva o HP/MP restantes do jogador no GameManager
            GameManager.instance.currentHP = playerUnit.currentHP;
            GameManager.instance.currentMP = playerUnit.currentMP;

            // Pega os valores ANTES de ganhar o XP
            int xpAntes = GameManager.instance.currentXP;
            int xpProximoNivelAntes = GameManager.instance.xpToNextLevel;
            int nivelAntes = GameManager.instance.playerLevel;

            // 2. Dá o XP ao GameManager (que pode causar um Level Up)
            GameManager.instance.GainXP(enemyUnit.xpValue);

            // Pega os valores DEPOIS de ganhar o XP
            int xpDepois = GameManager.instance.currentXP;
            int xpProximoNivelDepois = GameManager.instance.xpToNextLevel;
            int nivelDepois = GameManager.instance.playerLevel;

            // Ativa o painel de UI
            if (xpDisplayPanel != null)
            {
                xpDisplayPanel.SetActive(true);
                xpText.text = $"Ganhou {enemyUnit.xpValue} XP!";
                levelText.text = "Nível " + nivelAntes;

                // Configura a barra de XP para o estado ANTES do ganho
                xpSlider.maxValue = xpProximoNivelAntes;
                xpSlider.value = xpAntes;

                // Anima a barra de XP
                float tempoDecorrido = 0f;
                float duracaoAnimacao = 1.5f;
                int xpMeta = xpDepois;

                // Se houve level up, a animação é em duas partes
                if (nivelDepois > nivelAntes)
                {
                    // Parte 1: Anima até o final da barra (level up)
                    while (tempoDecorrido < duracaoAnimacao)
                    {
                        tempoDecorrido += Time.deltaTime;
                        xpSlider.value = Mathf.Lerp(xpAntes, xpProximoNivelAntes, tempoDecorrido / duracaoAnimacao);
                        yield return null;
                    }

                    // Som de Level Up, efeito visual, etc.
                    battleLogManager.AddLogMessage("LEVEL UP! Você é Nível " + nivelDepois + "!");
                    levelText.text = "Nível " + nivelDepois;

                    // Parte 2: A barra zera e preenche com o XP que sobrou
                    xpSlider.value = 0;
                    xpSlider.maxValue = xpProximoNivelDepois;
                    xpMeta = xpDepois; // O XP que sobrou
                    xpAntes = 0; // Começa do zero
                    tempoDecorrido = 0;
                }

                // Anima o XP restante (ou o ganho normal se não houve level up)
                while (tempoDecorrido < duracaoAnimacao)
                {
                    tempoDecorrido += Time.deltaTime;
                    xpSlider.value = Mathf.Lerp(xpAntes, xpMeta, tempoDecorrido / duracaoAnimacao);
                    yield return null;
                }
                xpSlider.value = xpMeta; // Garante que chegou ao valor final
            }

            // 3. Adiciona o inimigo à lista de derrotados
            GameManager.instance.defeatedEnemyIDs.Add(GameManager.instance.currentEnemyID);

            yield return new WaitForSeconds(2.5f);

            // 4. Verifica se a 'lastExplorationScene' está vazia (por causa do teste)
            if (string.IsNullOrEmpty(GameManager.instance.lastExplorationScene))
            {
                // Se estiver vazia, volta para o Menu por segurança
                GameManager.instance.LoadSceneWithFade("TitleScreen");
            }
            else
            {
                // Se estiver preenchida, volta para a cena de exploração (o fluxo normal)
                GameManager.instance.LoadSceneWithFade(GameManager.instance.lastExplorationScene);
            }
        }
        else if (state == BattleState.LOST)
        {
            battleLogManager.AddLogMessage("Você foi derrotado.");

            // Por enquanto, vamos esperar e carregar o menu principal
            yield return new WaitForSeconds(2f);
            GameManager.instance.LoadSceneWithFade("TitleScreen"); // Mude se o nome da sua cena for outro
        }
    }
    #endregion
}