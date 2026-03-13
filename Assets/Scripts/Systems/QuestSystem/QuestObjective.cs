using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public string descricao;
    public QuestObjectiveType tipo;

    // Para CollectItem / DeliverItem
    public DadosItem itemAlvo;
    public int quantidadeNecessaria;

    // Para KillEnemy
    [Tooltip("Arraste aqui o prefab do inimigo de EXPLORAÇÃO (que tem EnemyAIController com battlePrefab configurado). A comparação é feita pelo battlePrefab, não por string ID.")]
    public GameObject enemyPrefab; // ← GameObject aceito no ScriptableObject

    // Para TalkToNpc
    [Tooltip("Arraste aqui o GameObject do NPC com quem o jogador deve falar na cena.")]
    public GameObject npcAlvo;

    // Para EnterBattle
    [Tooltip("Arraste aqui o prefab do inimigo de EXPLORAÇÃO cujo combate precisa ser iniciado. A comparação é feita pelo battlePrefab, não por string ID.")]
    public GameObject battleEnemyPrefab; // ← idem

    // Para Timer
    public float timerNecessario;

    // Progresso em runtime
    public int progressoAtual;
    public float timerAtual;

    // Informativo — exibido no HUD mas não exigido para conclusão da quest
    [Tooltip("Se marcado, este objetivo é apenas informativo. A quest completa quando todos os objetivos não-informativos terminarem. Este objetivo nunca precisa ser completado.")]
    public bool apenasInformativo;

    public bool EstaCompleto()
    {
        if (tipo == QuestObjectiveType.Timer)
            return timerNecessario > 0f && timerAtual >= timerNecessario;

        int needed = quantidadeNecessaria > 0 ? quantidadeNecessaria : 1;
        return progressoAtual >= needed;
    }
}