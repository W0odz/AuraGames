using UnityEngine;

[CreateAssetMenu(menuName = "Game/QuestDefinition")]
public class QuestDefinition : ScriptableObject
{
    public string questId;
    public string questName;
    public string description;

    public QuestObjectiveType type;
    public DadosItem itemToCollect; // Para objetivo de coleta/entrega
    public int itemAmount;
    // Outros campos: inimigo, NPC alvo, etc.
    public DadosItem rewardItem;
    public int rewardAmount;
}