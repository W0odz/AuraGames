using UnityEngine;

public class PiercingMinigame : MonoBehaviour
{
    [Header("UI e Layout")]
    public RectTransform qteContainer; // O objeto pai que contém o Aro e o Alvo
    public RectTransform ringUI;
    public RectTransform actionOverlay; // Área total onde o aro pode aparecer


    [Header("Configurações do Jogo")]
    public LayerMask weakSpotLayer;

    [Header("Configurações de Spawn")]
    public float margemSeguranca = 200f; // Aumente esse valor no Inspector se continuar saindo da tela

    [Header("Configuração de Tolerância")]
    public float perfectScale = 0.3f;
    public float margemDeAcerto = 0.15f;
    public float startingScale = 3f;

    private bool isActive;
    private float currentScale;
    private float speed;

    private float cooldownAtivacao = 0.1f; // Pequena trava de segurança

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        // TRAVA DE SEGURANÇA: Impede que o clique no monstro acione o QTE instantaneamente
        if (cooldownAtivacao > 0)
        {
            cooldownAtivacao -= Time.deltaTime;
            return;
        }

        // Faz o aro encolher com o tempo
        currentScale -= speed * Time.deltaTime;

        if (ringUI != null)
        {
            ringUI.localScale = new Vector3(currentScale, currentScale, 1f);
        }

        // Detecta o clique do jogador para tentar acertar
        if (Input.GetMouseButtonDown(0))
        {
            ValidarAcerto();
        }
        // Se o aro ficar pequeno demais (passou do ponto e o jogador não clicou)
        else if (currentScale <= 0.05f)
        {
            Debug.Log("⏱️ QTE Falhou: O tempo acabou antes do clique!");
            Finalizar(false);
        }
    }

    private void OnDisable()
    {
        isActive = false;
        cooldownAtivacao = 0;
        currentScale = 3f; // Reseta o tamanho do aro para o padrão inicial
    }

    public void Iniciar(DadosArma arma, Vector2 posicaoDoClique)
    {
        // A CORREÇÃO ESTÁ AQUI: O WorldToScreenPoint traduz a coordenada do monstro para a UI do Canvas
        transform.position = Camera.main.WorldToScreenPoint(posicaoDoClique);

        // O resto continua igual...
        currentScale = startingScale;
        if (ringUI != null)
        {
            ringUI.localScale = new Vector3(currentScale, currentScale, 1f);
        }

        cooldownAtivacao = 0.2f;
        isActive = true;
        gameObject.SetActive(true);
    }

    void PosicionarAroAleatoriamente()
    {
        if (actionOverlay == null || qteContainer == null) return;

        // Calcula os limites subtraindo a sua margem de segurança do tamanho total do overlay
        float limiteX = (actionOverlay.rect.width / 2) - margemSeguranca;
        float limiteY = (actionOverlay.rect.height / 2) - margemSeguranca;

        // Trava de segurança: impede que a margem seja maior que a própria tela (evita erros)
        limiteX = Mathf.Max(0, limiteX);
        limiteY = Mathf.Max(0, limiteY);

        Vector2 novaPosicao;
        int tentativas = 0;

        // Sorteia a posição garantindo que fique dentro da safezone
        do
        {
            novaPosicao = new Vector2(Random.Range(-limiteX, limiteX), Random.Range(-limiteY, limiteY));
            tentativas++;
        }
        // Garante que não spawne exatamente no centro geométrico da tela sempre (opcional)
        while (Vector2.Distance(novaPosicao, Vector2.zero) < 100f && tentativas < 15);

        qteContainer.anchoredPosition = novaPosicao;
    }


    void ValidarAcerto()
    {
        // Define o tamanho máximo e mínimo aceitáveis baseados na sua margem
        float limiteMaximo = perfectScale + margemDeAcerto;
        float limiteMinimo = perfectScale - margemDeAcerto;

        // É SUCESSO SE: A escala atual for MENOR que o limite máximo E MAIOR que o limite mínimo
        bool sucesso = (currentScale <= limiteMaximo) && (currentScale >= limiteMinimo);

        // LOG DE CALIBRAÇÃO: Vai te ajudar a ajustar a "margemDeAcerto" perfeitamente no Inspector
        Debug.Log($"QTE Clicado! Escala Atual: {currentScale} | Limites Aceitos: {limiteMinimo} até {limiteMaximo} | Resultado: SUCESSO = {sucesso}");

        Finalizar(sucesso);
    }

    void Fail() => Finalizar(false);

    void Finalizar(bool sucesso)
    {
        isActive = false;
        AttackManager.Instance.FinalizarAtaque(sucesso);
    }
}