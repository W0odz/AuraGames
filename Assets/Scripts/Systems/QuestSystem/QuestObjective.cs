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
    public string enemyId; // Bate com EnemyAIController.enemyID

    // Para TalkToNpc
    public string npcName; // Bate com gameObject.name do NPC

    // Progresso em runtime (não serializado no SO, controlado pelo QuestManager)
    [System.NonSerialized] public int progressoAtual;

    public bool EstaCompleto()
    {
        int needed = quantidadeNecessaria > 0 ? quantidadeNecessaria : 1;
        return progressoAtual >= needed;
    }
}
