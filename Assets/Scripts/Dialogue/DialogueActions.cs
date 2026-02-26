using UnityEngine;

public static class DialogueActions
{
    public static void Execute(DialogueActionType action, string questId)
    {
        var manager = QuestManager.Instance;
        switch (action)
        {
            case DialogueActionType.StartQuest:
                manager.StartQuest(questId);
                break;
            case DialogueActionType.CompleteQuest:
                manager.CompleteQuest(questId);
                break;
            case DialogueActionType.TurnInQuest:
                manager.TurnInQuest(questId);
                break;
        }
    }
}