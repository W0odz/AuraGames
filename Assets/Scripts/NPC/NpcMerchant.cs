using UnityEngine;
using System;

[Serializable]
public class SlotEstoque
{
    public DadosItem item;
    [Min(1)] public int quantidade = 1;
}

public class NpcMerchant : MonoBehaviour
{
    [Header("Visual do Menu")]
    public Sprite fundoMenu; // ← null = usa o fundo padrão

    [Header("Escambo — Tolerância")]
    [Range(0f, 1f)]
    public float tolerancia = 0.15f;

    [Header("Estoque do Mercador")]
    public SlotEstoque[] estoque;

    [Header("Falas")]
    public string falaSaudacao = "O que você tem pra trocar?";
    public string falaMuitoBoa = "Opa... isso é um ótimo negócio pra mim!";
    public string falaBoa = "Hmm... isso me parece justo.";
    public string falaQuase = "Ainda está pouco... talvez se melhorar a oferta.";
    public string falaRuim = "Não, não, isso não vale nem perto do que você quer.";
    public string falaSemItens = "Você não tem nada pra me oferecer.";
    public string falaSucesso = "Negócio feito!";

    public string AvaliarComFala(int valorOferecido, int valorDesejado)
    {
        if (valorOferecido <= 0) return falaSemItens;

        var av = EscamboSystem.Avaliar(valorOferecido, valorDesejado, tolerancia);
        return av switch
        {
            EscamboSystem.AvaliacaoTroca.MuitoBoa => falaMuitoBoa,
            EscamboSystem.AvaliacaoTroca.Boa => falaBoa,
            EscamboSystem.AvaliacaoTroca.Quase => falaQuase,
            _ => falaRuim
        };
    }

    // Decrementa a quantidade após a troca — remove o slot se chegar a 0
    public void ConsumirDoEstoque(DadosItem item, int qty)
    {
        for (int i = 0; i < estoque.Length; i++)
        {
            if (estoque[i].item == item)
            {
                estoque[i].quantidade -= qty;
                if (estoque[i].quantidade <= 0)
                {
                    // Remove o slot zerando o item
                    estoque[i].item = null;
                    estoque[i].quantidade = 0;
                }
                return;
            }
        }
    }

    // Adiciona ao estoque os itens recebidos do jogador
    public void AdicionarAoEstoque(DadosItem item, int qty = 1)
    {
        for (int i = 0; i < estoque.Length; i++)
        {
            if (estoque[i].item == item)
            {
                estoque[i].quantidade += qty;
                return;
            }
        }

        // Item novo — expande o array
        System.Array.Resize(ref estoque, estoque.Length + 1);
        estoque[estoque.Length - 1] = new SlotEstoque { item = item, quantidade = qty };
    }

    public void OpenMerchantMenu()
    {
        if (MerchantMenuUI.Instance != null)
            MerchantMenuUI.Instance.Open(this);
        else
            Debug.LogWarning("[NpcMerchant] MerchantMenuUI.Instance não encontrado na cena!");
    }
}