using System.Collections.Generic;
using UnityEngine;

public static class EscamboSystem
{
    public enum AvaliacaoTroca { MuitoAcima, Acima, Ideal, Abaixo, MuitoAbaixo }

    public static AvaliacaoTroca Avaliar(int valorOferecido, int valorDesejado, float tolerancia)
    {
        if (valorDesejado <= 0 || valorOferecido <= 0) return AvaliacaoTroca.MuitoAbaixo;

        float ratio = (float)valorOferecido / valorDesejado;

        if (Mathf.Abs(ratio - 1f) < Mathf.Epsilon) // Ideal absoluto
            return AvaliacaoTroca.Ideal;

        if (ratio > 1f)
        {
            if (ratio - 1f <= tolerancia)
                return AvaliacaoTroca.Acima;
            else
                return AvaliacaoTroca.MuitoAcima;
        }
        else
        {
            if (1f - ratio <= tolerancia)
                return AvaliacaoTroca.Abaixo;
            else
                return AvaliacaoTroca.MuitoAbaixo;
        }
    }

    public static bool TrocaEhAceitavel(int valorOferecido, int valorDesejado, float tolerancia)
    {
        var av = Avaliar(valorOferecido, valorDesejado, tolerancia);
        return av == AvaliacaoTroca.Ideal || av == AvaliacaoTroca.Acima || av == AvaliacaoTroca.Abaixo;
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