using UnityEngine;

public class PlayerUnit : Unit
{
    public static PlayerUnit Instance;

    [Header("Progressão do Jogador")]
    public int playerLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

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
                    maxMP += item.bonusMaxMP;
                    strength += item.bonusStrength;
                    resistance += item.bonusResistance;
                }
            }
        }

        if (currentHP > maxHP) currentHP = maxHP;
    }
}