using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantMenuUI : MonoBehaviour
{
    public static MerchantMenuUI Instance;

    [Header("Painel raiz")]
    public GameObject painel;

    [Header("Estoque do NPC")]
    public Transform estoqueContent;
    public GameObject estoqueSlotPrefab;

    [Header("Oferta do Jogador")]
    public Transform ofertaContent;
    public GameObject ofertaSlotPrefab;

    [Header("Item desejado")]
    public Image iconeItemDesejado;
    public TextMeshProUGUI textoItemDesejado;

    [Header("Fala do NPC")]
    public TextMeshProUGUI textoFala;

    [Header("Botões")]
    public Button botaoConfirmar;
    public Button botaoFechar;

    [Header("Bloqueio de Input")]
    public GameObject bloqueadorInput;

    private NpcMerchant _merchant;
    private DadosItem _itemDesejado;
    private int _qtdDesejada = 1;
    private List<(DadosItem item, int qty)> _ofertaJogador = new();

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
        _itemDesejado = null;
        _qtdDesejada = 1;
        _ofertaJogador.Clear();

        PopularEstoque();
        RefreshOfertaUI();
        AtualizarItemDesejadoUI();

        textoFala.text = merchant.falaSaudacao;
        botaoConfirmar.interactable = false;

        if (bloqueadorInput != null) bloqueadorInput.SetActive(true);
        painel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Fechar()
    {
        if (bloqueadorInput != null) bloqueadorInput.SetActive(false);
        painel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Estoque do NPC ──────────────────────────────────────────────

    private void PopularEstoque()
    {
        foreach (Transform t in estoqueContent) Destroy(t.gameObject);
        if (_merchant.estoque == null) return;

        foreach (var item in _merchant.estoque)
        {
            if (item == null) continue;
            var go = Instantiate(estoqueSlotPrefab, estoqueContent);
            go.GetComponent<MerchantEstoqueSlotUI>().Setup(item, this);
        }
    }

    public void SelecionarItemDesejado(DadosItem item, int quantidade = 1)
    {
        _itemDesejado = item;
        _qtdDesejada = quantidade;
        AtualizarItemDesejadoUI();
        AtualizarFalaEBotao();
    }

    private void AtualizarItemDesejadoUI()
    {
        bool temItem = _itemDesejado != null;
        if (iconeItemDesejado != null)
        {
            iconeItemDesejado.sprite = temItem ? _itemDesejado.iconeItem : null;
            iconeItemDesejado.enabled = temItem;
        }
        if (textoItemDesejado != null)
            textoItemDesejado.text = temItem ? $"{_qtdDesejada}x {_itemDesejado.nomeItem}" : "Escolha um item";
    }

    // ── Oferta do Jogador ───────────────────────────────────────────

    public void AdicionarItemOferta(DadosItem item, int qty)
    {
        for (int i = 0; i < _ofertaJogador.Count; i++)
        {
            if (_ofertaJogador[i].item == item)
            {
                _ofertaJogador[i] = (item, _ofertaJogador[i].qty + qty);
                RefreshOfertaUI();
                AtualizarFalaEBotao();
                return;
            }
        }
        _ofertaJogador.Add((item, qty));
        RefreshOfertaUI();
        AtualizarFalaEBotao();
    }

    public void RemoverItemOferta(DadosItem item)
    {
        _ofertaJogador.RemoveAll(e => e.item == item);
        RefreshOfertaUI();
        AtualizarFalaEBotao();
    }

    private void RefreshOfertaUI()
    {
        foreach (Transform t in ofertaContent) Destroy(t.gameObject);
        foreach (var (item, qty) in _ofertaJogador)
        {
            var go = Instantiate(ofertaSlotPrefab, ofertaContent);
            go.GetComponent<MerchantOfertaSlotUI>().Setup(item, qty, this);
        }
    }

    // ── Avaliação em Tempo Real ─────────────────────────────────────

    private int CalcValorOferta()
    {
        int total = 0;
        foreach (var (item, qty) in _ofertaJogador)
            total += item.valorEscambo * qty;
        return total;
    }

    private void AtualizarFalaEBotao()
    {
        if (_itemDesejado == null)
        {
            textoFala.text = _merchant.falaSaudacao;
            botaoConfirmar.interactable = false;
            return;
        }

        int valorOferecido = CalcValorOferta();
        int valorDesejado = _itemDesejado.valorEscambo * _qtdDesejada;

        textoFala.text = _merchant.AvaliarComFala(valorOferecido, valorDesejado);
        botaoConfirmar.interactable = EscamboSystem.TrocaEhAceitavel(valorOferecido, valorDesejado, _merchant.tolerancia);
    }

    // ── Confirmar ───────────────────────────────────────────────────

    private void OnConfirmar()
    {
        int valorOferecido = CalcValorOferta();
        int valorDesejado = _itemDesejado.valorEscambo * _qtdDesejada;

        bool sucesso = EscamboSystem.ExecutarTroca(
            _itemDesejado, _qtdDesejada,
            _ofertaJogador,
            valorOferecido, valorDesejado,
            _merchant.tolerancia);

        if (sucesso)
        {
            textoFala.text = _merchant.falaSucesso;
            _ofertaJogador.Clear();
            RefreshOfertaUI();
            AtualizarFalaEBotao();
        }
    }
}