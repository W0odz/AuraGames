using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Controle Principal")]
    public GameObject inventoryRoot; // O painel "Menu Geral" que engloba tudo
    public bool isOpen = false;

    [Header("Paineis de Abas")]
    public GameObject backpackPanel;
    public GameObject craftingPanel;

    [Header("Grid da Mochila")]
    public Transform gridContent;
    public GameObject itemSlotPrefab;

    [Header("Status do Rodape (Icones)")]
    public TextMeshProUGUI txtTotalAttack;
    public TextMeshProUGUI txtTotalDefense;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Garante que o sistema comece fechado
        CloseInventory();
    }

    private void Update()
    {
        // Alterado de KeyCode.I para KeyCode.Tab conforme solicitado
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen) CloseInventory();
            else OpenInventory();
        }
    }

    public void OpenInventory()
    {
        isOpen = true;
        if (inventoryRoot != null) inventoryRoot.SetActive(true);
        Time.timeScale = 0f; // Pausa o jogo
        OpenBackpack();
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryRoot != null) inventoryRoot.SetActive(false);
        if (CraftingUIManager.Instance != null)
        {
            CraftingUIManager.Instance.ResetCraftingUI();
        }
    

        if (TooltipManager.Instance != null) TooltipManager.Instance.Hide();
        Time.timeScale = 1f; // Retoma o jogo
    }

    private void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += UpdateAll;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= UpdateAll;
    }

    public void UpdateAll()
    {
        if (!isOpen) return;

        UpdateGrid();
        UpdateFooterStats();
    }

    public void UpdateGrid()
    {
        if (gridContent == null || InventoryManager.Instance == null) return;

        foreach (Transform child in gridContent) Destroy(child.gameObject);

        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            GameObject go = Instantiate(itemSlotPrefab, gridContent);
            go.GetComponent<ItemSlotUI>().Setup(slot.item, slot.quantidade);
        }
    }

    private void UpdateFooterStats()
    {
        int totalAtk = 0;
        int totalDef = 0;

        if (EquipmentManager.Instance != null)
        {
            foreach (var item in EquipmentManager.Instance.currentEquipment)
            {
                if (item != null)
                {
                    totalAtk += item.bonusStrength;
                    totalDef += item.bonusResistance;
                }
            }
        }

        if (txtTotalAttack != null) txtTotalAttack.text = totalAtk.ToString();
        if (txtTotalDefense != null) txtTotalDefense.text = totalDef.ToString();
    }

    public void OpenBackpack()
    {
        if (backpackPanel != null) backpackPanel.SetActive(true);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        UpdateAll();
    }

    public void OpenCrafting()
    {
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(true);

        if (CraftingUIManager.Instance != null) CraftingUIManager.Instance.PopulateRecipeList();
    }
}