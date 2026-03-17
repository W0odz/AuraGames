using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Menu de itens embutido na caixa de diálogo — estilo Undertale.
/// Substitui o BattleItemPanelUI. Não usa painel separado.
/// </summary>
public class BattleItemInlineUI : MonoBehaviour
{
    public static BattleItemInlineUI Instance;

    [Header("Referências")]
    [Tooltip("O mesmo TextMeshProUGUI do dialogueText no BattleSystem")]
    public TextMeshProUGUI caixaDialogo;

    [Header("Configuração")]
    public int itensPorLinha = 2;
    public string prefixoSelecionado = "* ";
    public string prefixoNormal    = "  ";

    // ── estado interno ─────────────────────────────────────────────
    private List<(DadosItem item, int quantidade)> _itens = new();
    private int _indice = 0;
    private bool _ativo = false;

    // ── ciclo de vida ──────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (!_ativo) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))  Mover(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Mover(+1);
        if (Input.GetKeyDown(KeyCode.UpArrow))    Mover(-itensPorLinha);
        if (Input.GetKeyDown(KeyCode.DownArrow))  Mover(+itensPorLinha);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            ConfirmarSelecao();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            Fechar();
    }

    // ── API pública ────────────────────────────────────────────────
    public void Abrir()
    {
        if (BattleSystem.Instance.state != BattleState.PLAYERTURN) return;

        _itens.Clear();
        foreach (var slot in InventoryManager.Instance.listaItens)
        {
            if (slot.item == null || slot.quantidade <= 0) continue;
            if (slot.item.tipoItem != TipoItem.Consumivel)          continue;
            if (slot.item.apenasForaDeBatalha)                      continue;
            _itens.Add((slot.item, slot.quantidade));
        }

        if (_itens.Count == 0)
        {
            if (caixaDialogo != null)
                caixaDialogo.text = "Sem itens disponíveis.";
            return;
        }

        _indice = 0;
        _ativo  = true;

        // Esconde os botões de comando enquanto o menu de itens está aberto
        if (BattleHUD.Instance != null && BattleHUD.Instance.commandsPanel != null)
            BattleHUD.Instance.commandsPanel.SetActive(false);

        Renderizar();
    }

    public void Fechar()
    {
        _ativo = false;

        // Restaura os botões e o texto padrão
        if (BattleHUD.Instance != null && BattleHUD.Instance.commandsPanel != null)
            BattleHUD.Instance.commandsPanel.SetActive(true);

        if (BattleSystem.Instance != null && caixaDialogo != null)
            caixaDialogo.text = "O que " + BattleSystem.Instance.playerUnit.unitName + " fará?";
    }

    // ── lógica interna ─────────────────────────────────────────────
    private void Mover(int delta)
    {
        _indice = Mathf.Clamp(_indice + delta, 0, _itens.Count - 1);
        Renderizar();
    }

    private void Renderizar()
    {
        if (caixaDialogo == null) return;

        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < _itens.Count; i++)
        {
            var (item, qty) = _itens[i];

            string prefixo = (i == _indice) ? prefixoSelecionado : prefixoNormal;
            sb.Append($"{prefixo}{item.nomeItem} x{qty}");

            // Quebra de linha a cada `itensPorLinha` colunas
            bool ultimoDaLinha = ((i + 1) % itensPorLinha == 0);
            bool ultimo        = (i == _itens.Count - 1);
            if (!ultimo)
                sb.Append(ultimoDaLinha ? "\n" : "    ");
        }

        caixaDialogo.text = sb.ToString();
    }

    private void ConfirmarSelecao()
    {
        if (_indice < 0 || _indice >= _itens.Count) return;

        var (item, _) = _itens[_indice];

        // 1. Executa efeito
        item.Use(PlayerUnit.Instance.gameObject);

        // 2. Remove do inventário
        InventoryManager.Instance.RemoverItem(item, 1);

        // 3. Atualiza HP na HUD
        BattleSystem.Instance.playerHUD.UpdateHP(PlayerUnit.Instance.currentHP);

        // 4. Fecha o menu inline
        _ativo = false;
        if (BattleHUD.Instance != null && BattleHUD.Instance.commandsPanel != null)
            BattleHUD.Instance.commandsPanel.SetActive(false);

        // 5. Passa o turno
        BattleSystem.Instance.PassarTurnoAposItem();
    }
}
