using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public string descricao; // Texto exibido na UI: "Matar 3 Lobos"
    public QuestObjectiveType tipo;

    // Para CollectItem / DeliverItem
    public DadosItem itemAlvo;
    public int quantidadeNecessaria;

    // Para KillEnemy
    public string enemyId; // Bate com GameManager.currentEnemyID ou EnemyUnit.unitName

    // Para TalkToNpc
    public string npcName; // Bate com gameObject.name do NPC

    // Para EnterBattle
    [Tooltip("ID do inimigo cujo combate precisa ser iniciado. Bate com GameManager.currentEnemyID.")]
    public string battleEnemyId;

    // Para Timer (duração em segundos)
    public float timerNecessario;

    // Progresso em runtime (não serializado no SO, controlado pelo QuestManager)
    [System.NonSerialized] public int progressoAtual;
    [System.NonSerialized] public float timerAtual;

    public bool EstaCompleto()
    {
        if (tipo == QuestObjectiveType.Timer)
            return timerAtual >= timerNecessario && timerNecessario > 0f;

        int needed = quantidadeNecessaria > 0 ? quantidadeNecessaria : 1;
        return progressoAtual >= needed;
    }
}