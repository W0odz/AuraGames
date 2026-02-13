using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Painéis")]
    public GameObject mainContainer;
    public GameObject sidePanel;        // Painel de descrição
    public GameObject equipmentSummary; // Painel com WeaponRow, HelmetRow, etc.

    [Header("Lista")]
    public Transform content;
    public GameObject prefabItemGrid;
    public GameObject prefabItemEquipRow;   

    [Header("Detalhes (SidePanel)")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText, itemDescText;

    [Header("Sprites do Sistema")]
    public Sprite spriteVazioPadrao; 

    private SlotEquipamento slotFiltrado = SlotEquipamento.Nenhum;
    private bool abaEquipamentoAtiva = false;

    void Awake() => Instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ToggleInventario();
    }

    public void ToggleInventario()
    {
        bool abrir = !mainContainer.activeSelf;
        mainContainer.SetActive(abrir);
        if (abrir) OnClickAbaItens(); // Abre sempre na aba de itens por padrão
    }

    // --- ABA 1: ITENS GERAIS ---
    public void OnClickAbaItens()
    {
        abaEquipamentoAtiva = false;
        slotFiltrado = SlotEquipamento.Nenhum;

        sidePanel.SetActive(true);
        equipmentSummary.SetActive(false);
        AtualizarVisual();
    }

    // --- ABA 2: EQUIPAMENTOS ---
    public void OnClickAbaEquipamentos()
    {
        abaEquipamentoAtiva = true;

        sidePanel.SetActive(false);
        equipmentSummary.SetActive(true);

        // Limpa a lista lateral até que um slot (Row) seja clicado
        foreach (Transform child in content) Destroy(child.gameObject);
    }

    // Chamado pelas Rows (WeaponRow, HelmetRow, etc.)
    public void SelecionarSlotParaEquipar(int slotIndex)
    {
        slotFiltrado = (SlotEquipamento)slotIndex;
        AtualizarVisual();
    }

    public void AtualizarVisual()
    {
        if (content == null || InventoryManager.Instance == null) return;

        // 1. LIMPEZA TOTAL: Remove tudo o que estava no scroll antes de reconstruir
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform child = content.GetChild(i);
            child.SetParent(null); // Desvincula na hora
            Destroy(child.gameObject);
        }

        // 2. SEÇÃO DE EQUIPAMENTO: Criar o botão "Nenhum" no TOPO
        if (abaEquipamentoAtiva)
        {
            GameObject btnNenhum = Instantiate(prefabItemEquipRow, content);
            btnNenhum.GetComponent<EquipmentSlotUI>().ConfigurarNenhum(slotFiltrado, spriteVazioPadrao);
        }

        // 3. LOOP DE ITENS: Gera o restante da lista
        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (!abaEquipamentoAtiva)
            {
                // --- ABA DE ITENS COMUNS (Consumíveis, etc) ---
                if (slot.item.tipoItem == TipoItem.Equipamento) continue;

                GameObject novo = Instantiate(prefabItemGrid, content);
                novo.GetComponent<ItemSlotUI>().ConfigurarSlot(slot.item, slot.quantidade);
            }
            else
            {
                // --- ABA DE EQUIPAMENTOS (Troca de Peças) ---

                // Filtro 1: Só mostra itens que cabem neste slot (Ex: só espadas)
                if (slot.item.slotOndeEquipa != slotFiltrado) continue;

                // Filtro 2: RESOLVE SEU ERRO - Se o item já estiver equipado, não mostramos ele na lista
                // Assim, se você equipou a DevSword, ela "some" da lista e vai para o painel central
                if (EquipmentManager.Instance.equipamentosAtuais[(int)slotFiltrado] == slot.item) continue;

                GameObject novo = Instantiate(prefabItemEquipRow, content);
                novo.GetComponent<EquipmentSlotUI>().Configurar(slot.item, slot.quantidade);
            }
        }
    }

    // Reação ao clique no Ícone (Slot)
    public void AoClicarNoIcone(DadosItem item)
    {
        if (!abaEquipamentoAtiva)
        {
            // Na aba de itens, apenas mostra a descrição
            sidePanel.SetActive(true);
            itemNameText.text = item.nomeItem;
            itemDescText.text = item.descricao;
            itemIcon.sprite = item.iconeItem;
        }
        else
        {
            // Na aba de equipamentos, o clique já EQUIPA o item
            EquipmentManager.Instance.Equipar(item);
            AtualizarVisual(); // Atualiza a lista (pra mostrar o check de equipado)
        }
    }
}