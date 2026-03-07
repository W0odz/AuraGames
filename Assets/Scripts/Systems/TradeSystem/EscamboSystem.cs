using System.Collections.Generic;
using UnityEngine;

public static class EscamboSystem
{
    public enum AvaliacaoTroca { MuitoBoa, Boa, Quase, Ruim }

    public static AvaliacaoTroca Avaliar(int valorOferecido, int valorDesejado, float tolerancia)
    {
        if (valorDesejado <= 0 || valorOferecido <= 0) return AvaliacaoTroca.Ruim;

        float ratio = (float)valorOferecido / valorDesejado;

        if (ratio >= 1.0f) return AvaliacaoTroca.MuitoBoa;
        if (ratio >= 1f - tolerancia) return AvaliacaoTroca.Boa;
        if (ratio >= 1f - tolerancia * 2f) return AvaliacaoTroca.Quase;
        return AvaliacaoTroca.Ruim;
    }

    public static bool TrocaEhAceitavel(int valorOferecido, int valorDesejado, float tolerancia)
    {
        var av = Avaliar(valorOferecido, valorDesejado, tolerancia);
        return av == AvaliacaoTroca.Boa || av == AvaliacaoTroca.MuitoBoa;
    }

    public static bool ExecutarTroca(
        DadosItem itemDesejado, int qtdDesejada,
        List<(DadosItem item, int qty)> ofertaJogador,
        int valorOferecido, int valorDesejado, float tolerancia)
    {
        if (!TrocaEhAceitavel(valorOferecido, valorDesejado, tolerancia)) return false;

        foreach (var (item, qty) in ofertaJogador)
            if (InventoryManager.Instance.GetItemCount(item) < qty) return false;

        foreach (var (item, qty) in ofertaJogador)
            InventoryManager.Instance.RemoverItem(item, qty);

        InventoryManager.Instance.AdicionarItem(itemDesejado, qtdDesejada);
        return true;
    }
}