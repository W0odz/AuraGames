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
}