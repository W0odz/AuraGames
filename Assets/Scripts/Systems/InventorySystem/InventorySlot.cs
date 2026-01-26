[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot(Item novoItem, int qtd)
    {
        item = novoItem;
        quantity = qtd;
    }

    public void AddQuantity(int valor) => quantity += valor;
    public void RemoveQuantity(int valor) => quantity -= valor;
}