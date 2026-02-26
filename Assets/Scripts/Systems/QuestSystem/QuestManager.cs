using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

public class QuestManager : MonoBehaviour
{
    private Dictionary<string, QuestState> questStates = new();

    public static QuestManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void StartQuest(string questId)
    {
        if (!questStates.ContainsKey(questId))
            questStates[questId] = QuestState.Active;
    }

    public bool IsActive(string questId) => questStates.TryGetValue(questId, out var state) && state == QuestState.Active;

    public void CompleteQuest(string questId)
    {
        if (IsActive(questId))
            questStates[questId] = QuestState.Completed;
    }

    public void TurnInQuest(string questId)
    {
        if (questStates.TryGetValue(questId, out var state) && state == QuestState.Completed)
            questStates[questId] = QuestState.TurnedIn;
    }
}
public enum QuestState { NotStarted, Active, Completed, TurnedIn }
public enum QuestObjectiveType { CollectItem, KillEnemy, TalkToNpc }