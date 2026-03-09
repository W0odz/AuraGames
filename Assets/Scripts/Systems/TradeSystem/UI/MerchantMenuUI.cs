using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantMenuUI : MonoBehaviour
{
    public static MerchantMenuUI Instance;

    [Header("Fundo do Menu")]
    public Image imagemFundo;
    public Sprite fundoPadrao;  // ← arrasta o sprite padrão no Inspector

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

        // Troca o fundo — usa o do mercador se tiver, senão o padrão
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
            // Ignora slots vazios ou zerados
            if (slot.item == null || slot.quantidade <= 0) continue;

            var go = Instantiate(itemSlotPrefab, estoqueContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                slot.item,
                () => AdicionarBarraMercador(slot.item, 1),
                slot.quantidade   // ← passa a qtd pro slot mostrar
            );
        }
    }

    private void PopularInventario()
    {
        foreach (Transform t in inventarioContent) Destroy(t.gameObject);
        if (InventoryManager.Instance.listaItens == null) return;

        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == null) continue;
            var go = Instantiate(itemSlotPrefab, inventarioContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(slot.item, () => AdicionarBarraJogador(slot.item, 1));
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
                RefreshEstoqueTemporario();  // ← atualiza grid
                AtualizarFalaEBotao();
                return;
            }
        }
        _barraMercador.Add((item, qty));
        RefreshBarraUI();
        RefreshEstoqueTemporario();  // ← atualiza grid
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
                RefreshEstoqueTemporario();  // ← atualiza grid
                AtualizarFalaEBotao();
                return;
            }
        }
    }

    // ── Barra Jogador ───────────────────────────────────────────────

    public void AdicionarBarraJogador(DadosItem item, int qty)
    {
        for (int i = 0; i < _barraJogador.Count; i++)
        {
            if (_barraJogador[i].item == item)
            {
                _barraJogador[i] = (item, _barraJogador[i].qty + qty);
                RefreshBarraUI();
                RefreshInventarioTemporario();  // ← atualiza grid
                AtualizarFalaEBotao();
                return;
            }
        }
        _barraJogador.Add((item, qty));
        RefreshBarraUI();
        RefreshInventarioTemporario();  // ← atualiza grid
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
                RefreshInventarioTemporario();  // ← atualiza grid
                AtualizarFalaEBotao();
                return;
            }
        }
    }

    // ── Refresh Temporário (descontando o que está na barra) ────────

    private void RefreshEstoqueTemporario()
    {
        foreach (Transform t in estoqueContent) Destroy(t.gameObject);
        if (_merchant.estoque == null) return;

        foreach (var slot in _merchant.estoque)
        {
            if (slot.item == null || slot.quantidade <= 0) continue;

            // Desconta o que já está na barra do mercador
            int naFarra = 0;
            foreach (var (item, qty) in _barraMercador)
                if (item == slot.item) naFarra = qty;

            int qtdDisponivel = slot.quantidade - naFarra;
            if (qtdDisponivel <= 0) continue; // item esgotado na mesa, some do grid

            var go = Instantiate(itemSlotPrefab, estoqueContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                slot.item,
                () => AdicionarBarraMercador(slot.item, 1),
                qtdDisponivel
            );
        }
    }

    private void RefreshInventarioTemporario()
    {
        foreach (Transform t in inventarioContent) Destroy(t.gameObject);
        if (InventoryManager.Instance.listaItens == null) return;

        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == null) continue;

            // Desconta o que já está na barra do jogador
            int naFarra = 0;
            foreach (var (item, qty) in _barraJogador)
                if (item == slot.item) naFarra = qty;

            int qtdDisponivel = slot.quantidade - naFarra;
            if (qtdDisponivel <= 0) continue; // item esgotado na mochila, some do grid

            var go = Instantiate(itemSlotPrefab, inventarioContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(
                slot.item,
                () => AdicionarBarraJogador(slot.item, 1),
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
            var go = Instantiate(itemSlotPrefab, barraMercadorContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(item, () => RemoverBarraMercador(item));
        }

        // lado jogador
        foreach (Transform t in barraJogadorContent) Destroy(t.gameObject);
        foreach (var (item, qty) in _barraJogador)
        {
            var go = Instantiate(itemSlotPrefab, barraJogadorContent);
            go.GetComponent<MerchantItemSlotUI>().Setup(item, () => RemoverBarraJogador(item));
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

        if (valorMercador == 0 && valorJogador == 0)
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
            _barraMercador[0].item, _barraMercador[0].qty,
            _barraJogador,
            valorJogador, valorMercador,
            _merchant.tolerancia);

        if (sucesso)
        {
            // Consome do estoque do mercador
            foreach (var (item, qty) in _barraMercador)
                _merchant.ConsumirDoEstoque(item, qty);

            // Adiciona ao estoque do mercador os itens do jogador
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