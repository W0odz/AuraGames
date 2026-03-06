using UnityEngine;

public class NpcMerchant : MonoBehaviour
{
    [Header("Escambo")]
    [Range(0f, 1f)]
    public float tolerancia = 0.15f;
    // 0.15 = aceita oferta com 15% a menos que o valor do item desejado

    public string falaBoa = "Hmm... isso me parece justo.";
    public string falaQuase = "Ainda está pouco... talvez se melhorar a oferta.";
    public string falaRuim = "Não, não, isso não vale nem perto do que você quer.";
    public string falaMuitoBoa = "Opa... isso é um ótimo negócio pra mim!";

    public void OpenMerchantMenu()
    {
        // TODO: Chamar MerchantMenu.Instance.Open quando o sistema de escambo estiver pronto.
    }
}