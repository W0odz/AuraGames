using System;
using UnityEngine;

// Enum já existente para facilitar seleção no inspector
public enum DialogueSide { Left, Right }

[Serializable]
public class DialogueNode
{
    [TextArea(2, 5)]
    public string text;

    public DialogueSide speakerSide;   // 'Left' ou 'Right'
    public string speakerName;         // Nome que aparece na caixa de nome
    public Sprite portrait;            // Sprite/expressão do personagem falando

    [Header("Ação de Quest (opcional)")]
    [Tooltip("Ação a ser executada quando o jogador AVANÇA (pressiona E) neste node.")]
    public DialogueActionType acaoDeQuest = DialogueActionType.None;
    [Tooltip("ID da quest que será acionada. Deve bater com QuestDefinition.questId.")]
    public string questId;
}