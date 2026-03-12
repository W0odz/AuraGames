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
    [Tooltip("Arraste aqui o prefab do inimigo de EXPLORAÇÃO (pasta Assets).")]
    public GameObject enemyPrefab; // ← GameObject aceito no ScriptableObject

    // Para TalkToNpc
    public string npcName;

    // Para EnterBattle
    [Tooltip("Arraste aqui o prefab do inimigo de EXPLORAÇÃO (pasta Assets).")]
    public GameObject battleEnemyPrefab; // ← idem

    // Para Timer
    public float timerNecessario;

    // Progresso em runtime
    public int progressoAtual;
    public float timerAtual;

    public bool EstaCompleto()
    {
        if (tipo == QuestObjectiveType.Timer)
            return timerNecessario > 0f && timerAtual >= timerNecessario;

        int needed = quantidadeNecessaria > 0 ? quantidadeNecessaria : 1;
        return progressoAtual >= needed;
    }
}