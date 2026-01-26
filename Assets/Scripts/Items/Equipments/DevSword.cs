using UnityEngine;

[CreateAssetMenu(fileName = "New Dev Sword", menuName = "Inventory/Equipment/Dev Sword")]
public class DevSwordItem : EquipmentItem
{
    // We can still keep the unique feedback message
    public string godModeMessage = "GOD MODE ACTIVATED!";

    public override void Use()
    {
        // This calls EquipmentManager.Instance.Equip(this) from the base class
        base.Use();

        Debug.Log($"<color=cyan>{godModeMessage}</color>");

        // If you still want to set Level and HP to 999 (things that aren't modifiers)
        ApplyExtraGodStats();
    }

    private void ApplyExtraGodStats()
    {
        GameManager gm = GameManager.instance;
        if (gm != null)
        {
            gm.playerLevel = 99;
            gm.currentHP = gm.maxHP = 999;
            gm.currentMP = gm.maxMP = 999;
            attackModifier = 999;
            defenseModifier = 999;
            speedModifier = 999;
        }
    }
}