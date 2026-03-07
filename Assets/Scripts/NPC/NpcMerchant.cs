using UnityEngine;

public class NpcMerchant : MonoBehaviour
{
    [Header("Escambo — Tolerância")]
    [Range(0f, 1f)]
    public float tolerancia = 0.15f;

    [Header("Estoque do Mercador")]
    public DadosItem[] estoque;

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

    public void OpenMerchantMenu()
    {
        if (MerchantMenuUI.Instance != null)
            MerchantMenuUI.Instance.Open(this);
        else
            Debug.LogWarning("[NpcMerchant] MerchantMenuUI.Instance não encontrado na cena!");
    }
}