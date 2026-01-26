using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory")]
public class Item : ScriptableObject
{
    public string id;
    public string itemName;
    [TextArea] public string descryption;
    public Sprite icon;
    public bool stackable;
    public int maxStack = 1;

    // Método virtual para que cada item tenha um comportamento diferente ao ser usado
    public virtual void Use()
    {
        Debug.Log("Usando " + itemName);
    }
}