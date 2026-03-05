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

    // Flag que persiste durante toda a sessão do save
    private bool primeiraArmaEquipada = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
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

        if (CollectionLogManager.Instance != null)
            CollectionLogManager.Instance.AddLog(item, quantidade);

        // Auto-equipa a primeira arma coletada no save
        TentarAutoEquiparArma(item);

        onUpdateUI?.Invoke();
    }

    private void TentarAutoEquiparArma(DadosItem item)
    {
        // Já equipou uma arma antes nesse save? Ignora
        if (primeiraArmaEquipada) return;

        // O item é uma arma?
        if (item is DadosArma && item.slotOndeEquipa == SlotEquipamento.Weapon)
        {
            // Slot de arma (índice 0) está vazio?
            if (EquipmentManager.Instance != null &&
                EquipmentManager.Instance.currentEquipment[0] == null)
            {
                primeiraArmaEquipada = true;
                EquipmentManager.Instance.Equip(item);
                Debug.Log($"[InventoryManager] Primeira arma auto-equipada: {item.nomeItem}");
            }
        }
    }

    // Chamado pelo sistema de Save ao carregar um save existente
    // para evitar que a arma seja auto-equipada novamente
    public void MarcarPrimeiraArmaJaEquipada()
    {
        primeiraArmaEquipada = true;
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
        foreach (var slot in listaItens)
        {
            if (slot.item == itemProcurado)
                total += slot.quantidade;
        }
        return total;
    }

    public bool TemItem(string nomeDoItem)
    {
        foreach (var slot in listaItens)
        {
            if (slot.item != null && slot.item.nomeItem == nomeDoItem)
                return true;
        }
        return false;
    }
}