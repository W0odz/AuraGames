using UnityEngine;
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY }

public class BattleSystem : MonoBehaviour
{
    [Header("Configurações do Inimigo")]
    public GameObject enemyPrefab;
    public Transform enemyBattleStation;
    Unit enemyUnit;

    [Header("Dados do Jogador")]
    // Arraste aqui um objeto que tenha o script Unit com os status do jogador
    public Unit playerUnit;

    [Header("Interface (HUD)")]
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    public TextMeshProUGUI dialogueText;

    public BattleState state;

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
        enemyGO.transform.SetParent(enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

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
        if (state != BattleState.PLAYERTURN) return;
        state = BattleState.BUSY;

        if (BattleHUD.Instance.commandsPanel != null)
            BattleHUD.Instance.commandsPanel.SetActive(false);

        AttackManager.Instance.IniciarSequenciaDeAtaque();
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

    void EndBattle()
    {
        if (state == BattleState.WON) dialogueText.text = "Vitória!";
        else if (state == BattleState.LOST) dialogueText.text = "Derrota...";
    }
}