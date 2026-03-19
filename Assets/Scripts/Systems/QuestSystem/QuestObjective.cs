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
    [Tooltip("Arraste aqui o prefab do inimigo de exploração.")]
    public GameObject enemyPrefab;

    // Para TalkToNpc
    [Tooltip("ID do NPC (componente NpcIdentidade.npcId).")]
    public string npcAlvoNome;

    // Para EnterBattle
    [Tooltip("Prefab do inimigo de exploração cujo combate precisa ser iniciado.")]
    public GameObject battleEnemyPrefab;

    // Para TriggerDialogue
    [Tooltip("ID do DialogueTrigger que precisa ser ativado. Deve bater com o campo 'triggerId' no DialogueTrigger.")]
    public string triggerDialogueId;

    // Para UseSceneTransition
    [Tooltip("ID do SceneTransition (campo transitionID) que precisa ser usado.")]
    public string sceneTransitionID;

    // Para AguardarDialogo
    [Tooltip("DialogueAsset cujo término completa este objetivo.")]
    public DialogueAsset dialogoAlvo;

    // Para Timer
    public float timerNecessario;

    // Progresso em runtime
    public int progressoAtual;
    public float timerAtual;

    [Tooltip("Se marcado, este objetivo é apenas informativo e nunca precisa ser completado.")]
    public bool apenasInformativo;

    public bool EstaCompleto()
    {
        if (tipo == QuestObjectiveType.Timer)
            return timerNecessario > 0f && timerAtual >= timerNecessario;

        int needed = quantidadeNecessaria > 0 ? quantidadeNecessaria : 1;
        return progressoAtual >= needed;
    }
}