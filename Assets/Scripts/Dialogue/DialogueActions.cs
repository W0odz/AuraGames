using UnityEngine;

public static class DialogueActions
{
    public static void Execute(DialogueActionType action, QuestDefinition def)
    {
        if (action == DialogueActionType.None) return;

        var manager = QuestManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[DialogueActions] QuestManager.Instance é null.");
            return;
        }

        if (def == null)
        {
            Debug.LogWarning($"[DialogueActions] Ação '{action}' ignorada — questDef não atribuído no DialogueNode.");
            return;
        }

        switch (action)
        {
            case DialogueActionType.StartQuest:
                manager.StartQuest(def);
                break;
            case DialogueActionType.CompleteQuest:
                manager.CompleteQuest(def.questId);
                break;
            case DialogueActionType.TurnInQuest:
                manager.TurnInQuest(def.questId);
                break;
            case DialogueActionType.DeliverItems:
                manager.TurnInQuest(def.questId);
                break;
        }
    }
}