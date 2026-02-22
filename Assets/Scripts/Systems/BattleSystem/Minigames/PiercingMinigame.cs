using UnityEngine;

public class PiercingMinigame : MonoBehaviour
{
    [Header("UI e Layout")]
    public RectTransform qteContainer;    // pai do QTE
    public RectTransform ringUI;          // aro que encolhe
    public RectTransform targetUI;        // alvo fixo
    public RectTransform actionOverlay;   // área total onde o QTE pode aparecer

    [Header("Configurações do Jogo")]
    public LayerMask weakSpotLayer;

    [Header("Configurações de Spawn")]
    public float margemSeguranca = 200f;

    [Header("Configuração Visual")]
    public float startingScale = 3f;

    [Header("Timing")]
    public float duracaoQte = 1.0f; // segundos para ir de startingScale até ~0

    [Header("Dificuldade / validação")]
    [Tooltip("Se true, valida pelo tamanho real (pixels) na tela. Recomendo deixar true.")]
    public bool usarValidacaoPorPixels = true;

    [Tooltip("Versão fácil: conta sucesso quando o aro estiver dentro do alvo (não precisa encostar na borda).")]
    public bool modoFacilDentroDoAlvo = true;

    [Tooltip("Margem em pixels para facilitar (aumenta a janela de sucesso).")]
    public float toleranciaPixels = 8f;

    [Header("LEGADO - por escala (opcional)")]
    public float perfectScale = 0.3f;
    public float margemDeAcerto = 0.15f;

    private bool isActive;
    private float currentScale;
    private float speed;
    private float cooldownAtivacao = 0.1f;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        if (cooldownAtivacao > 0f)
        {
            cooldownAtivacao -= Time.deltaTime;
            return;
        }

        currentScale -= speed * Time.deltaTime;
        currentScale = Mathf.Max(0f, currentScale);

        if (ringUI != null)
            ringUI.localScale = new Vector3(currentScale, currentScale, 1f);

        if (Input.GetMouseButtonDown(0))
        {
            ValidarAcerto();
        }
        else if (currentScale <= 0.05f)
        {
            Debug.Log("⏱️ QTE Falhou: O tempo acabou antes do clique!");
            Finalizar(false);
        }
    }

    private void OnDisable()
    {
        isActive = false;
        cooldownAtivacao = 0f;
        currentScale = startingScale;
        speed = 0f;
    }

    public void Iniciar(DadosArma arma, Vector2 posicaoDoClique)
    {
        transform.position = Camera.main.WorldToScreenPoint(posicaoDoClique);

        currentScale = startingScale;
        speed = (duracaoQte <= 0f) ? 0f : (startingScale / duracaoQte);

        if (ringUI != null)
            ringUI.localScale = new Vector3(currentScale, currentScale, 1f);

        cooldownAtivacao = 0.2f;
        isActive = true;
        gameObject.SetActive(true);
    }

    void PosicionarAroAleatoriamente()
    {
        if (actionOverlay == null || qteContainer == null) return;

        float limiteX = (actionOverlay.rect.width / 2f) - margemSeguranca;
        float limiteY = (actionOverlay.rect.height / 2f) - margemSeguranca;

        limiteX = Mathf.Max(0, limiteX);
        limiteY = Mathf.Max(0, limiteY);

        Vector2 novaPosicao;
        int tentativas = 0;

        do
        {
            novaPosicao = new Vector2(Random.Range(-limiteX, limiteX), Random.Range(-limiteY, limiteY));
            tentativas++;
        }
        while (Vector2.Distance(novaPosicao, Vector2.zero) < 100f && tentativas < 15);

        qteContainer.anchoredPosition = novaPosicao;
    }

    void ValidarAcerto()
    {
        bool sucesso = usarValidacaoPorPixels ? ValidarAcertoPorPixels() : ValidarAcertoPorEscalaLegado();
        Finalizar(sucesso);
    }

    bool ValidarAcertoPorEscalaLegado()
    {
        float limiteMaximo = perfectScale + margemDeAcerto;
        float limiteMinimo = perfectScale - margemDeAcerto;

        bool sucesso = (currentScale <= limiteMaximo) && (currentScale >= limiteMinimo);

        Debug.Log($"QTE (ESCALA) | escala={currentScale:F3} | limites={limiteMinimo:F3}..{limiteMaximo:F3} | sucesso={sucesso}");
        return sucesso;
    }

    bool ValidarAcertoPorPixels()
    {
        if (ringUI == null || targetUI == null)
        {
            Debug.LogWarning("QTE (PIXELS) falhou: ringUI ou targetUI não setados no Inspector.");
            return false;
        }

        // Raios em pixels na tela
        float ringRadius = (ringUI.rect.width * ringUI.lossyScale.x) * 0.5f;
        float targetRadius = (targetUI.rect.width * targetUI.lossyScale.x) * 0.5f;

        bool sucesso;
        if (modoFacilDentroDoAlvo)
        {
            // Versão fácil:
            // Sucesso se o aro estiver dentro do alvo (ou até um pouquinho maior pela tolerância).
            sucesso = ringRadius <= (targetRadius + toleranciaPixels);
        }
        else
        {
            // Versão “na borda” (mais difícil):
            // Sucesso se os raios forem aproximadamente iguais.
            float diff = Mathf.Abs(ringRadius - targetRadius);
            sucesso = diff <= toleranciaPixels;
        }

        Debug.Log($"QTE (PIXELS) | ringR={ringRadius:F2}px targetR={targetRadius:F2}px tol={toleranciaPixels:F2}px | facil={modoFacilDentroDoAlvo} | sucesso={sucesso}");
        return sucesso;
    }

    void Finalizar(bool sucesso)
    {
        isActive = false;
        AttackManager.Instance.FinalizarAtaque(sucesso);
    }
}