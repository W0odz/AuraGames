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

    [Header("Força de Vontade")]
    public bool temForcaDeVontade = true;


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
        // Salva o HP atual antes de inicializar (persiste entre batalhas)
        int hpAnterior = currentHP;

        // Copia os stats do GameManager (já incluem bônus de equipamento)
        if (GameManager.Instance != null)
        {
            maxHP = GameManager.Instance.maxHP;
            maxMP = GameManager.Instance.maxMP;
            strength = GameManager.Instance.strength;
            resistance = GameManager.Instance.resistance;
        }

        base.InicializarUnidade(); // Define currentHP = maxHP

        // Se tinha HP salvo de uma batalha anterior, restaura ele (sem ultrapassar o maxHP)
        if (hpAnterior > 0 && hpAnterior < maxHP)
            currentHP = hpAnterior;
        else if (currentHP > maxHP)
            currentHP = maxHP;
    }

    public void RestaurarForcaDeVontade()
    {
        temForcaDeVontade = true;
        Debug.Log("[PlayerUnit] Força de Vontade restaurada!");
    }


    public bool ConsumirForcaDeVontade()
    {
        if (!temForcaDeVontade) return false;
        temForcaDeVontade = false;
        Debug.Log("[PlayerUnit] Força de Vontade consumida!");
        return true;
    }

    #region Debuffs

    public int GetEffectiveStrength()
    {
        int str = strength;

        if (HasDebuff(DebuffType.StrengthUp))
        {
            int stacks = GetDebuffStacks(DebuffType.StrengthUp);
            str += 10 * stacks; // cada stack adiciona 10 de força — ajusta como quiser
        }

        return str;
    }

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

    public bool RemoverDebuffAleatorio()
    {
        // Filtra só os debuffs ativos
        var ativos = debuffs.FindAll(d => d.turnsLeft > 0);
        if (ativos.Count == 0) return false;

        // Escolhe um aleatório e remove
        var escolhido = ativos[Random.Range(0, ativos.Count)];
        debuffs.Remove(escolhido);

        Debug.Log($"[PlayerUnit] Debuff removido: {escolhido.type}");
        return true;
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

    /// <summary>Adiciona XP ao jogador e processa level up automaticamente.</summary>
    public void AdicionarXP(int quantidade)
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.GainXP(quantidade);

        // Sincroniza os campos do PlayerUnit com o GameManager (usado pela barra de XP da batalha)
        currentXP = GameManager.Instance.currentXP;
        xpToNextLevel = GameManager.Instance.xpToNextLevel;
        playerLevel = GameManager.Instance.playerLevel;

        Debug.Log($"[PlayerUnit] +{quantidade} XP ganho pela quest. XP atual: {currentXP}/{xpToNextLevel}");
    }
}