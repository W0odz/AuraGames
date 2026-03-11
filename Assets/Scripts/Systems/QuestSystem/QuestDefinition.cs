using System.Collections.Generic;
using UnityEngine;

public enum QuestType { MainQuest, SideQuest, Daily }

[CreateAssetMenu(fileName = "Nova Quest", menuName = "Quests/QuestDefinition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Identificação")]
    public string questId;
    public string questName;
    [TextArea] public string description;
    public QuestType questType;

    [Header("NPC")]
    public string npcDadorNome;   // gameObject.name do NPC que dá a quest
    public string npcEntregaNome; // gameObject.name do NPC de entrega (pode ser igual)

    [Header("Pré-requisitos")]
    public List<QuestDefinition> questsNecessarias; // Quests que devem estar TurnedIn antes

    [Header("Objetivos")]
    public List<QuestObjective> objetivos;

    [Header("Recompensa")]
    public int recompensaXP = 50;
}