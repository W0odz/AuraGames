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
        if (InventoryManager.Instance != null)
        {
            // Apontamos para o RefreshUI (que é seguro) e não para o UpdateUI
            InventoryManager.Instance.onInventoryChangedCallback += RefreshUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback -= RefreshUI;
        }
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
            if (slotEntry != null && slotEntry.item != null && slotPrefab != null)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsParent);

                if (newSlot != null)
                {
                    newSlot.transform.localScale = Vector3.one;
                    InventorySlotUI ui = newSlot.GetComponent<InventorySlotUI>();
                    if (ui != null) ui.AddItem(slotEntry.item);
                }
            }
        }
    }
}