using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia o painel de itens na batalha.
/// Abre ao clicar em ITENS, lista consumíveis com quantidade > 0.
/// </summary>
public class BattleItemPanelUI : MonoBehaviour
{
    public static BattleItemPanelUI Instance;

    [Header("Referências")]
    public GameObject painel;
    public Transform listaContent;       // O "Content" do ScrollRect
    public GameObject itemSlotPrefab;    // Prefab do BattleItemSlotUI
    public Button botaoFechar;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painel.SetActive(false);
    }

    void Start()
    {
        botaoFechar.onClick.AddListener(Fechar);
    }

    public void Abrir()
    {
        // Só abre no turno do jogador
        if (BattleSystem.Instance.state != BattleState.PLAYERTURN) return;

        AtualizarLista();
        painel.SetActive(true);
    }

    public void Fechar()
    {
        painel.SetActive(false);
    }

    public void AtualizarLista()
    {
        foreach (Transform filho in listaContent)
            Destroy(filho.gameObject);

        if (InventoryManager.Instance == null) return;

        bool temAlgumItem = false;

        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == null) continue;
            if (slot.quantidade <= 0) continue;
            if (slot.item.tipoItem != TipoItem.Consumivel) continue;

            // Esconde itens que só podem ser usados fora de batalha (ex: Fogueira)
            if (slot.item.apenasForaDeBatalha) continue;

            GameObject go = Instantiate(itemSlotPrefab, listaContent);
            go.GetComponent<BattleItemSlotUI>().Setup(slot.item, slot.quantidade);
            temAlgumItem = true;
        }

        if (!temAlgumItem) Fechar();
    }
}