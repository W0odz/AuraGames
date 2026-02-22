using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Configurações Estratégicas")]
    public bool foiDescoberto = false;
    public int usosMaximos = 3; // Quantas vezes o jogador pode usar este ponto
    private int usosRestantes;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        usosRestantes = usosMaximos;

        // Começa a batalha 100% invisível
        if (sr != null) sr.enabled = false;
    }

    void Update()
    {
        // NOVA REGRA DE VISIBILIDADE:
        // Fica visível o tempo todo (independente do turno) SE foi descoberto E ainda tiver usos.
        if (sr != null)
        {
            sr.enabled = (foiDescoberto && usosRestantes > 0);
        }
    }

    // Agora retorna 'true' se o acerto for válido, e 'false' se os usos já tiverem acabado
    public bool ReceberClique()
    {
        if (usosRestantes > 0)
        {
            if (!foiDescoberto)
            {
                Debug.Log("Ponto Fraco DESCOBERTO! Revelando na tela.");
                foiDescoberto = true;
            }

            usosRestantes--; // Consome um uso
            Debug.Log("Acertou! Usos restantes do ponto fraco: " + usosRestantes);

            return true; // Retorna sucesso para aplicar o multiplicador de dano
        }

        Debug.Log("Este ponto fraco já foi totalmente destruído/esgotado.");
        return false; // Retorna falha para o jogador dar apenas o dano normal
    }
}