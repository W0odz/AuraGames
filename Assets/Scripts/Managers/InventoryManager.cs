using System.Collections.Generic;
using UnityEngine;
using static InventoryManager;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class Slot
    {
        public DadosItem item;
        public int quantidade;
        public Slot(DadosItem i, int q) { item = i; quantidade = q; }
    }

    public List<Slot> listaItens = new List<Slot>();
    public delegate void OnUpdateUI();
    public OnUpdateUI onUpdateUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // Isso mantém os itens equipados entre as cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdicionarItem(DadosItem item, int quantidade)
    {
        Slot slotExistente = listaItens.Find(s => s.item == item);
        if (slotExistente != null) slotExistente.quantidade += quantidade;
        else listaItens.Add(new Slot(item, quantidade));

        onUpdateUI?.Invoke();
    }

    public void RemoverItem(DadosItem item, int quantidade)
    {
        Slot slotExistente = listaItens.Find(s => s.item == item);
        if (slotExistente != null)
        {
            slotExistente.quantidade -= quantidade;
            if (slotExistente.quantidade <= 0) listaItens.Remove(slotExistente);
        }
        onUpdateUI?.Invoke();
    }

    public int GetItemCount(DadosItem itemProcurado)
    {
        int total = 0;

        // SUBSTITUA 'nomeDaSuaLista' pelo nome real que está no seu script
        foreach (var slot in listaItens)
        {
            // Se a sua lista for de 'ItemSlot', use: slot.item
            // Se for uma lista direta de 'DadosItem', use: slot == itemProcurado
            if (slot.item == itemProcurado)
            {
                total += slot.quantidade;
            }
        }

        return total;
    }

    public bool TemItem(string nomeDoItem)
    {
        // Percorre a lista de itens que você já tem no script
        foreach (var slot in listaItens)
        {
            if (slot.item != null && slot.item.nomeItem == nomeDoItem)
            {
                return true; // Encontrou a gema!
            }
        }
        return false; // Não está na mochila
    }
}