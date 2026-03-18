using UnityEngine;

[CreateAssetMenu(menuName = "Game/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    [Header("Retratos fixos (opcional)")]
    public Sprite portraitEsquerda;   // ← aparece sempre na esquerda (pode ser null)
    public Sprite portraitDireita;    // ← aparece sempre na direita (pode ser null)

    [Header("Fundo do painel (opcional)")]
    [Tooltip("Sprite exibido como fundo do painel de diálogo. Deixe vazio para fundo invisível.")]
    public Sprite fundoPainel;

    public DialogueNode[] nodes;
}