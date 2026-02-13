public class PlayerUnit : Unit
{
    public static PlayerUnit Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetupPlayerStats(GameManager gm)
    {
        // 1. Puxa do GameManager 
        unitName = gm.playerName;
        playerLevel = gm.playerLevel;
        maxHP = gm.maxHP;
        maxMP = gm.maxMP;
        currentHP = gm.currentHP;
        currentMP = gm.currentMP;
        strength = gm.strength;
        resistance = gm.resistance;
        will = gm.will;
        knowledge = gm.knowledge;
        speed = gm.speed;
        luck = gm.luck;
    }

    public override void InicializarUnidade()
    {
        // 1. Puxa os dados base do GameManager
        if (GameManager.instance != null)
        {
            SetupPlayerStats(GameManager.instance);
        }

        // 2. Soma Equipamentos
        if (EquipmentManager.Instance != null)
        {
            foreach (var item in EquipmentManager.Instance.equipamentosAtuais)
            {
                if (item == null) continue;
                strength += item.bonusStrength;
                resistance += item.bonusResistance;
                will += item.bonusWill;
                knowledge += item.bonusKnowledge;
                speed += item.bonusSpeed;
                luck += item.bonusLuck;
                maxHP += item.bonusMaxHP;
                maxMP += item.bonusMaxMP;
            }
        }

        base.InicializarUnidade();
    }
}