using UnityEngine;
using System;

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

    [Header("Recompensas ao fim do diálogo (opcional)")]
    public RecompensaDialogo[] recompensas;
}

[Serializable]
public class RecompensaDialogo
{
    public enum TipoRecompensa { ConcederItem, EquiparItem }

    [Tooltip("ConcederItem: adiciona ao inventário. EquiparItem: adiciona ao inventário e equipa diretamente.")]
    public TipoRecompensa tipo;

    public DadosItem item;

    [Min(1)]
    [Tooltip("Quantidade de itens concedidos. Ignorado para EquiparItem (sempre equipa 1 unidade).")]
    public int quantidade = 1;
}