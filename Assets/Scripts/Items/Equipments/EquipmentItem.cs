using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentItem : Item
{
    public EquipmentSlot equipSlot;

    [Header("Stats Modifiers")]
    public int attackModifier;
    public int defenseModifier;
    public int speedModifier;

    public override void Use()
    {
        base.Use();
        // When used from the inventory, equip this item
        EquipmentManager.Instance.Equip(this);
    }
}