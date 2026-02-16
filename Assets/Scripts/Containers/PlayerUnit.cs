using UnityEngine;

public class PlayerUnit : Unit
{
    public static PlayerUnit Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Adicione 'override' aqui para resolver o aviso CS0114
    public override void InicializarUnidade()
    {
        // 1. Opcional: Chama a lógica original da classe Unit (reset de HP/MP)
        base.InicializarUnidade();

        // 2. Soma os bônus dos equipamentos do novo layout
        if (EquipmentManager.Instance != null)
        {
            foreach (var item in EquipmentManager.Instance.currentEquipment)
            {
                if (item != null)
                {
                    // Usa os nomes em inglês do seu script DadosItem
                    maxHP += item.bonusMaxHP;
                    maxMP += item.bonusMaxMP;
                    strength += item.bonusStrength;
                    resistance += item.bonusResistance;
                }
            }
        }

        // 3. Garante que o HP não passe do novo limite
        if (currentHP > maxHP) currentHP = maxHP;
    }
}