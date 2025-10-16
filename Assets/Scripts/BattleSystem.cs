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

    public BattleLogManager battleLogManager;
    public Button attackButton;
    public Button defendButton;
    public Button magicButton;

    public GameObject magicMenuPanel;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    // Start é chamado antes do primeiro frame
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    void SetActionButtons(bool isActive)
    {
        attackButton.interactable = isActive;
        defendButton.interactable = isActive;
        magicButton.interactable = isActive;
    }

    // Coroutine para configurar a batalha
    IEnumerator SetupBattle()
    {
        // Instancia o jogador e o inimigo na cena
        // MODIFICADO: Instanciamos o jogador na posição EXATA do Battle Station
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation.position, Quaternion.identity);
        playerUnit = playerGO.GetComponent<Unit>();
        playerGO.transform.parent = playerBattleStation;

        enemyGO = Instantiate(enemyPrefab, enemyBattleStation.position, Quaternion.identity);
        enemyUnit = enemyGO.GetComponent<Unit>();
        enemyGO.transform.parent = enemyBattleStation;

        battleLogManager.AddLogMessage("Um " + enemyUnit.unitName + " selvagem apareceu!");

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        playerHUD.UpdateMP(playerUnit.currentMP);

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
    }

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

    IEnumerator PlayerAttack()
    {
        // Desabilita o botão para evitar múltiplos cliques
        attackButton.interactable = false;
        SetActionButtons(false); // Desabilita todos os botões

        // Causa dano no inimigo
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        enemyHUD.UpdateHP(enemyUnit.currentHP);

        battleLogManager.AddLogMessage($"{playerUnit.unitName} ataca e causa {playerUnit.damage} de dano!");
        yield return new WaitForSeconds(1f);

        // Atualiza a vida do inimigo (aqui você colocaria uma barra de vida)
        // HUD.SetHP(enemyUnit.currentHP);

        if (isDead)
        {
            state = BattleState.WON;
            yield return StartCoroutine(enemyUnit.FadeOut());
            enemyGO.SetActive(false);
            EndBattle();
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

    IEnumerator PlayerMagicAttack()
    {
        int manaCost = 20;
        int magicDamage = 40;

        // 1. Verifica se tem mana suficiente
        if (playerUnit.currentMP < manaCost)
        {
            battleLogManager.AddLogMessage("Mana insuficiente!");
            yield break; // Interrompe a coroutine
        }

        // 2. Se tiver mana, executa a ação
        SetActionButtons(false);

        playerUnit.currentMP -= manaCost;
        playerHUD.UpdateMP(playerUnit.currentMP);

        battleLogManager.AddLogMessage($"{playerUnit.unitName} usa uma bola de fogo!");
        yield return new WaitForSeconds(1f);

        bool isDead = enemyUnit.TakeDamage(magicDamage);
        enemyHUD.UpdateHP(enemyUnit.currentHP);
        battleLogManager.AddLogMessage($"O ataque causa {magicDamage} de dano!");
        yield return new WaitForSeconds(1f);

        // 3. Checa o resultado da batalha
        if (isDead)
        {
            state = BattleState.WON;
            yield return StartCoroutine(enemyUnit.FadeOut());
            enemyGO.SetActive(false);
            EndBattle();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerHeal()
    {
        int manaCost = 15;
        int healAmount = 30;

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

        playerUnit.Heal(healAmount);
        playerHUD.UpdateHP(playerUnit.currentHP);

        battleLogManager.AddLogMessage($"{playerUnit.unitName} se cura em {healAmount} pontos de vida!");
        yield return new WaitForSeconds(1f);

        // 3. Passa o turno para o inimigo
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        battleLogManager.AddLogMessage($"{enemyUnit.unitName} ataca e causa {enemyUnit.damage} de dano!");
        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

        playerHUD.UpdateHP(playerUnit.currentHP);

        playerUnit.isDefending = false;

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

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            battleLogManager.AddLogMessage("Você venceu a batalha!") ;
        }
        else if (state == BattleState.LOST)
        {
            battleLogManager.AddLogMessage("Você foi derrotado.") ;
        }
    }
}