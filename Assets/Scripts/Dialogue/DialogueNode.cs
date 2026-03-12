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
    [Tooltip("Ação a ser executada quando o jogador AVANÇA além deste node (pressiona E).")]
    public DialogueActionType acaoDeQuest = DialogueActionType.None;
    [Tooltip("QuestDefinition que será acionada. Arraste o ScriptableObject aqui.")]
    public QuestDefinition questDef;

    [Header("Filtro de Estado de Quest (opcional)")]
    [Tooltip("Define em qual estado da quest este nó será exibido. 'Sempre' ignora o filtro.")]
    public QuestStateFilter estadoQuest = QuestStateFilter.Sempre;
}