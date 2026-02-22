using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    [Header("Tabela de Recompensas")]
    public List<Loot> tabelaDeLoot;
    public int expReward;

    public override void InicializarUnidade()
    {
        // Inimigos usam os valores que você digitou no Inspector da Unity
        base.InicializarUnidade();
    }
}

[System.Serializable]
public class Loot
{
    public DadosItem item; // O Scriptable Object do item (ex: Gema, Couro, etc.)
    public int quantidade = 1;
    [Range(0, 100)] public float chanceDeDrop = 100f; // 100% significa que sempre cai
}