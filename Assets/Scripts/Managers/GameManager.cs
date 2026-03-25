// Cria um novo jogo (usa valores padrão)
public void CreateNewGame(string playerNameInput, string cenaInicial = "Vila_01") {
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
