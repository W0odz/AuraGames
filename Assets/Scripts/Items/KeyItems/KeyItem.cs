using UnityEngine;

[CreateAssetMenu(fileName = "New Key Item", menuName = "Inventory/Items/Key Item")]
public class KeyItem : Item
{
    [Header("Key Settings")]
    public string keyType; // Ex: "BlueGem", "RedCard", etc.

    public override void Use()
    {
        base.Use();
        Debug.Log($"The {itemName} is a passive item. It's used automatically at doors.");
    }
}