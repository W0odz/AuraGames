using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    public static PlayerUnit Instance;

    [Header("Progressão do Jogador")]
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Atributos")]
    public int agility = 10;

    [Header("Debuffs (protótipo)")]
    [SerializeField] private List<DebuffInstance> debuffs = new();

    [System.Serializable]
    private class DebuffInstance
    {
        public DebuffType type;
        public int turnsLeft;
        public int stacks;
    }

    private void Awake()
    {
        // singleton + persistência + anti-duplicata
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void InicializarUnidade()
    {
        base.InicializarUnidade();

        if (EquipmentManager.Instance != null)
        {
            foreach (var item in EquipmentManager.Instance.currentEquipment)
            {
                if (item != null)
                {
                    maxHP += item.bonusMaxHP;
                    strength += item.bonusStrength;

                    // DECISÃO DO PROJETO: resistência vem exclusivamente do status da arma/equipamento
                    resistance = item.bonusResistance;
                }
            }
        }

        if (currentHP > maxHP) currentHP = maxHP;
    }

    #region Debuffs
    public void ApplyDebuff(DebuffType type, int turns, int stacks = 1)
    {
        if (type == DebuffType.None) return;
        if (turns <= 0) return;
        stacks = Mathf.Max(1, stacks);

        var existing = debuffs.Find(d => d.type == type);
        if (existing != null)
        {
            existing.turnsLeft = Mathf.Max(existing.turnsLeft, turns);
            existing.stacks += stacks;
        }
        else
        {
            debuffs.Add(new DebuffInstance
            {
                type = type,
                turnsLeft = turns,
                stacks = stacks
            });
        }
    }

    public bool HasDebuff(DebuffType type)
    {
        return debuffs.Exists(d => d.type == type && d.turnsLeft > 0);
    }



    public int GetDebuffStacks(DebuffType type)
    {
        var d = debuffs.Find(x => x.type == type && x.turnsLeft > 0);
        return d != null ? d.stacks : 0;
    }

    public void TickDebuffsOnPlayerTurnStart()
    {
        for (int i = debuffs.Count - 1; i >= 0; i--)
        {
            debuffs[i].turnsLeft--;
            if (debuffs[i].turnsLeft <= 0)
                debuffs.RemoveAt(i);
        }
    }

    // Helpers de "stats efetivos" (assim o BattleSystem não precisa saber detalhes)
    public int GetEffectiveAgility()
    {
        int agi = agility;

        // Exemplo: EvasionDown reduz agilidade efetiva por stacks
        if (HasDebuff(DebuffType.EvasionDown))
        {
            int stacks = GetDebuffStacks(DebuffType.EvasionDown);
            agi -= 5 * stacks;
        }

        return Mathf.Max(0, agi);
    }

    public float GetDamageMultiplierFromDebuffs()
    {
        float mult = 1f;

        // Exemplo: Weakness reduz dano por stacks
        if (HasDebuff(DebuffType.Weakness))
        {
            int stacks = GetDebuffStacks(DebuffType.Weakness);
            // 1 stack = -30%, 2 stacks = -60% (clamp pra não zerar)
            mult *= Mathf.Clamp01(1f - 0.30f * stacks);
        }

        return mult;
    }
    #endregion
}