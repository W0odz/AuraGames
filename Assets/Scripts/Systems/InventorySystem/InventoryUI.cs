using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Configurações de UI")]
    public GameObject inventoryPanel;
    public Transform itemsParent;
    public GameObject slotPrefab;

    private bool isInventoryOpen = false;

    // 1. Inscrição correta nos eventos
    private void OnEnable()
    {
        // Escuta mudanças no inventário (pegar/soltar itens)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onInventoryChangedCallback += RefreshUI;

        // Escuta mudanças no equipamento (equipar/desequipar)
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += OnEquipmentChangedRefresh;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onInventoryChangedCallback -= RefreshUI;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= OnEquipmentChangedRefresh;
    }

    // Método auxiliar para o evento de equipamento
    void OnEquipmentChangedRefresh(EquipmentItem newItem, EquipmentItem oldItem)
    {
        RefreshUI();
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        RefreshUI(); // Atualiza uma vez ao iniciar
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            RefreshUI();
        }
    }

    // 2. O "Gatilho" seguro para a atualização
    public void RefreshUI()
    {
        Debug.Log("[InventoryUI] Sinal de atualização recebido! Iniciando Coroutine...");
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(UpdateRoutine());
        }
    }

    // 3. A Coroutine que evita o erro de MissingReferenceException
    IEnumerator UpdateRoutine()
    {
        // Espera o final do processamento do frame (evita conflito de clique)
        yield return new WaitForEndOfFrame();

        if (itemsParent == null || InventoryManager.Instance == null) yield break;

        // Limpeza dos slots antigos
        for (int i = itemsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsParent.GetChild(i).gameObject);
        }

        // Criar novos slots baseados na lista do Manager
        foreach (var slotEntry in InventoryManager.Instance.inventory)
        {
            if (slotEntry.item != null)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsParent);
                InventorySlotUI ui = newSlot.GetComponent<InventorySlotUI>();

                // PERGUNTA AO MANAGER: Esse item está equipado?
                bool equipped = EquipmentManager.Instance.IsItemEquipped(slotEntry.item);

                Debug.Log($"Desenhando slot para {slotEntry.item.itemName}. Equipado: {equipped}"); // ADICIONE ISSO

                // Passa essa info para o AddItem do slot
                ui.AddItem(slotEntry.item, equipped);
            }
        }
    }
}