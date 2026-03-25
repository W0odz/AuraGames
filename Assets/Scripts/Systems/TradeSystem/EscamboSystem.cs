using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de escambo (troca de itens) entre jogador e mercador.
///
/// Lógica de avaliação baseada em ratio = valorOferecido / valorDesejado:
///   ratio >= 1 + tolerancia*2  => MuitoAcima   (jogador oferece muito mais — suspeito)
///   ratio >= 1 + tolerancia    => Acima         (oferta generosa — aceitável)
///   ratio >= 1 - tolerancia    => Ideal         (troca equilibrada — aceitável)
///   ratio >= 1 - tolerancia*2  => Abaixo        (oferta baixa — recusado)
///                              => MuitoAbaixo   (oferta irrisória — recusado)
///
/// Somente Ideal e Acima são aceitos. MuitoAcima também é BLOQUEADO para
/// proteger o jogador de ser enganado.
/// </summary>
public static class EscamboSystem
{
    public enum AvaliacaoTroca { MuitoAcima, Acima, Ideal, Abaixo, MuitoAbaixo }

    public static AvaliacaoTroca Avaliar(int valorOferecido, int valorDesejado, float tolerancia)
    {
        if (valorDesejado <= 0 || valorOferecido <= 0)
            return AvaliacaoTroca.MuitoAbaixo;

        float ratio = (float)valorOferecido / valorDesejado;

        if (ratio >= 1f + tolerancia * 2f) return AvaliacaoTroca.MuitoAcima;
        if (ratio >= 1f + tolerancia)      return AvaliacaoTroca.Acima;
        if (ratio >= 1f - tolerancia)      return AvaliacaoTroca.Ideal;
        if (ratio >= 1f - tolerancia * 2f) return AvaliacaoTroca.Abaixo;
        return AvaliacaoTroca.MuitoAbaixo;
    }

    /// <summary>
    /// Só permite Ideal ou Acima. Bloqueia MuitoAcima (proteção ao jogador)
    /// e Abaixo/MuitoAbaixo (proteção ao mercador).
    /// </summary>
    public static bool TrocaEhAceitavel(int valorOferecido, int valorDesejado, float tolerancia)
    {
        var av = Avaliar(valorOferecido, valorDesejado, tolerancia);
        return av == AvaliacaoTroca.Ideal || av == AvaliacaoTroca.Acima;
    }

    /// <summary>
    /// Executa a troca: remove os itens do jogador e entrega os itens do mercador.
    /// Retorna false se a troca não for aceitável ou se o jogador não tiver os itens.
    /// </summary>
    public static bool ExecutarTroca(
        List<(DadosItem item, int qty)> ofertaMercador,
        List<(DadosItem item, int qty)> ofertaJogador,
        int valorOferecido, int valorDesejado, float tolerancia)
    {
        if (!TrocaEhAceitavel(valorOferecido, valorDesejado, tolerancia))
            return false;

        // Verifica se o jogador realmente tem todos os itens
        foreach (var (item, qty) in ofertaJogador)
            if (InventoryManager.Instance.GetItemCount(item) < qty)
                return false;

        // Remove os itens do jogador
        foreach (var (item, qty) in ofertaJogador)
            InventoryManager.Instance.RemoverItem(item, qty);

        // Entrega os itens do mercador ao jogador
        foreach (var (item, qty) in ofertaMercador)
            InventoryManager.Instance.AdicionarItem(item, qty);

        return true;
    }
}