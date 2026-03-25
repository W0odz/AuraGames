using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantMenuUI : MonoBehaviour
{
    public static MerchantMenuUI Instance;

    [Header("Fundo do Menu")]
    public Image imagemFundo;
    public Sprite fundoPadrao;

    [Header("Painel raiz")]
    public GameObject painel;

    [Header("Estoque do NPC")]
    public Transform estoqueContent;

    [Header("Inventário do Jogador")]
    public Transform inventarioContent;

    [Header("Slot Prefab (compartilhado)")]
    public GameObject itemSlotPrefab;

    [Header("Barra de Troca — lado Mercador")]
    public Transform barraMercadorContent;

    [Header("Barra de Troca — lado Jogador")]
    public Transform barraJogadorContent;

    [Header("Fala do NPC")]
    public TextMeshProUGUI textoFala;

    [Header("Botões")]
    public Button botaoConfirmar;
    public Button botaoFechar;

    [Header("Bloqueio de Input")]
    public GameObject bloqueadorInput;

    private NpcMerchant _merchant;
    private List<(DadosItem item, int qty)> _barraJogador = new();
    private List<(DadosItem item, int qty)> _barraMercador = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        painel.SetActive(false);
    }

    void Start()
    {
        botaoConfirmar.onClick.AddListener(OnConfirmar);
        botaoFechar.onClick.AddListener(Fechar);
    }

    // ── Abrir / Fechar ──────────────────────────────────────────────

    public void Open(NpcMerchant merchant)
    {
        _merchant = merchant;
        _barraJogador.Clear();
        _barraMercador.Clear();

        if (imagemFundo != null)
            imagemFundo.sprite = merchant.fundoMenu != null ? merchant.fundoMenu : fundoPadrao;

        PopularEstoque();
        PopularInventario();
        RefreshBarraUI();

        textoFala.text = merchant.falaSaudacao;
        botaoConfirmar.interactable = false;

        GameManager.Instance.inputBloqueado = true;
        painel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Fechar()
    {
        if (bloqueadorInput != null) bloqueadorInput.SetActive(false);
        GameManager.Instance.inputBloqueado = false;
        painel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Popular Grids ───────────────────────────────────────────────

    private void PopularEstoque()
    {
        foreach (Transform t in estoqueContent) Destroy(t.gameObject);
        if (_merchant.estoque == null) return;

        foreach (var slot in _merchant.estoque)
        {
            if (slot.item == null || slot.quantidade <= 0) continue;

            DadosItem itemCapturado = slot.item;
            var go = Instantiate(itemSlotPrefab, estoqueContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                itemCapturado,
                () => AdicionarBarraMercador(itemCapturado, 1),
                slot.quantidade
            );
        }
    }

    private void PopularInventario()
    {
        foreach (Transform t in inventarioContent) Destroy(t.gameObject);
        if (InventoryManager.Instance.listaItens == null) return;

        // Usa um HashSet para evitar duplicatas caso o mesmo item apareça em múltiplos slots
        var itemsAdicionados = new System.Collections.Generic.HashSet<DadosItem>();

        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == null) continue;
            if (slot.item.naoVendivel) continue;
            if (itemsAdicionados.Contains(slot.item)) continue; // já foi processado

            DadosItem itemCapturado = slot.item;
            itemsAdicionados.Add(itemCapturado);

            // Usa GetItemCount para obter a quantidade real no inventário
            int qtyNoInventario = InventoryManager.Instance.GetItemCount(itemCapturado);
            if (qtyNoInventario <= 0) continue; // item zerado — não exibe

            // Desconta o que já está na barra do jogador
            int naOferta = 0;
            foreach (var (barItem, barQty) in _barraJogador)
                if (barItem == itemCapturado) { naOferta = barQty; break; }

            int qtdDisponivel = qtyNoInventario - naOferta;
            if (qtdDisponivel <= 0) continue; // todos exemplares já estão na oferta

            var go = Instantiate(itemSlotPrefab, inventarioContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                itemCapturado,
                () => AdicionarBarraJogador(itemCapturado, 1),
                qtdDisponivel
            );
        }
    }

    // ── Barra Mercador ──────────────────────────────────────────────

    public void AdicionarBarraMercador(DadosItem item, int qty)
    {
        for (int i = 0; i < _barraMercador.Count; i++)
        {
            if (_barraMercador[i].item == item)
            {
                _barraMercador[i] = (item, _barraMercador[i].qty + qty);
                RefreshBarraUI();
                RefreshEstoqueTemporario();
                AtualizarFalaEBotao();
                return;
            }
        }
        _barraMercador.Add((item, qty));
        RefreshBarraUI();
        RefreshEstoqueTemporario();
        AtualizarFalaEBotao();
    }

    public void RemoverBarraMercador(DadosItem item)
    {
        for (int i = 0; i < _barraMercador.Count; i++)
        {
            if (_barraMercador[i].item == item)
            {
                int novaQty = _barraMercador[i].qty - 1;
                if (novaQty <= 0) _barraMercador.RemoveAt(i);
                else _barraMercador[i] = (item, novaQty);

                RefreshBarraUI();
                RefreshEstoqueTemporario();
                AtualizarFalaEBotao();
                return;
            }
        }
    }

    // ── Barra Jogador ───────────────────────────────────────────────

    public void AdicionarBarraJogador(DadosItem item, int qty)
    {
        // Garante que o jogador não ofereça mais do que possui
        int noInventario = InventoryManager.Instance.GetItemCount(item);
        int jaOfertado = 0;
        foreach (var (barItem, barQty) in _barraJogador)
            if (barItem == item) { jaOfertado = barQty; break; }

        if (jaOfertado + qty > noInventario) return;

        for (int i = 0; i < _barraJogador.Count; i++)
        {
            if (_barraJogador[i].item == item)
            {
                _barraJogador[i] = (item, _barraJogador[i].qty + qty);
                RefreshBarraUI();
                PopularInventario();
                AtualizarFalaEBotao();
                return;
            }
        }
        _barraJogador.Add((item, qty));
        RefreshBarraUI();
        PopularInventario();
        AtualizarFalaEBotao();
    }

    public void RemoverBarraJogador(DadosItem item)
    {
        for (int i = 0; i < _barraJogador.Count; i++)
        {
            if (_barraJogador[i].item == item)
            {
                int novaQty = _barraJogador[i].qty - 1;
                if (novaQty <= 0) _barraJogador.RemoveAt(i);
                else _barraJogador[i] = (item, novaQty);

                RefreshBarraUI();
                PopularInventario();
                AtualizarFalaEBotao();
                return;
            }
        }
    }

    // ── Refresh Temporário do Estoque ───────────────────────────────

    private void RefreshEstoqueTemporario()
    {
        foreach (Transform t in estoqueContent) Destroy(t.gameObject);
        if (_merchant.estoque == null) return;

        foreach (var slot in _merchant.estoque)
        {
            if (slot.item == null || slot.quantidade <= 0) continue;

            int naOferta = 0;
            foreach (var (barItem, barQty) in _barraMercador)
                if (barItem == slot.item) naOferta = barQty;

            int qtdDisponivel = slot.quantidade - naOferta;
            if (qtdDisponivel <= 0) continue;

            DadosItem itemCapturado = slot.item;
            var go = Instantiate(itemSlotPrefab, estoqueContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                itemCapturado,
                () => AdicionarBarraMercador(itemCapturado, 1),
                qtdDisponivel
            );
        }
    }

    // ── Refresh Barra ───────────────────────────────────────────────

    private void RefreshBarraUI()
    {
        // lado mercador
        foreach (Transform t in barraMercadorContent) Destroy(t.gameObject);
        foreach (var (item, qty) in _barraMercador)
        {
            DadosItem itemCapturado = item;
            int qtyCapturada = qty;
            var go = Instantiate(itemSlotPrefab, barraMercadorContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(itemCapturado, () => RemoverBarraMercador(itemCapturado), qtyCapturada);
        }

        // lado jogador
        foreach (Transform t in barraJogadorContent) Destroy(t.gameObject);
        foreach (var (item, qty) in _barraJogador)
        {
            DadosItem itemCapturado = item;
            int qtyCapturada = qty;
            var go = Instantiate(itemSlotPrefab, barraJogadorContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(itemCapturado, () => RemoverBarraJogador(itemCapturado), qtyCapturada);
        }
    }

    // ── Avaliação ───────────────────────────────────────────────────

    private int CalcValorLado(List<(DadosItem item, int qty)> lista)
    {
        int total = 0;
        foreach (var (item, qty) in lista)
            total += item.valorEscambo * qty;
        return total;
    }

    private void AtualizarFalaEBotao()
    {
        int valorJogador = CalcValorLado(_barraJogador);
        int valorMercador = CalcValorLado(_barraMercador);

        if (valorMercador == 0 || valorJogador == 0)
        {
            textoFala.text = _merchant.falaSaudacao;
            botaoConfirmar.interactable = false;
            return;
        }

        textoFala.text = _merchant.AvaliarComFala(valorJogador, valorMercador);
        botaoConfirmar.interactable = EscamboSystem.TrocaEhAceitavel(valorJogador, valorMercador, _merchant.tolerancia);
    }

    // ── Confirmar ───────────────────────────────────────────────────
    private void OnConfirmar()
    {
        int valorJogador = CalcValorLado(_barraJogador);
        int valorMercador = CalcValorLado(_barraMercador);

        bool sucesso = EscamboSystem.ExecutarTroca(
            _barraMercador,
            _barraJogador,
            valorJogador, valorMercador,
            _merchant.tolerancia);

        if (sucesso)
        {
            foreach (var (item, qty) in _barraMercador)
                _merchant.ConsumirDoEstoque(item, qty);

            foreach (var (item, qty) in _barraJogador)
                _merchant.AdicionarAoEstoque(item, qty);

            textoFala.text = _merchant.falaSucesso;
            _barraJogador.Clear();
            _barraMercador.Clear();
            RefreshBarraUI();
            PopularEstoque();
            PopularInventario();
            AtualizarFalaEBotao();
        }
    }
}