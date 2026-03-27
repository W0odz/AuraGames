using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquestra uma sequência cinemática scripted dentro de uma cena:
/// 
///   1. Diálogo imediato ao entrar na cena (disparado automaticamente no Start).
///   2. Fade Out → reposiciona / troca sprites de personagens no pico do preto → Fade In.
///   3. Diálogo final logo em seguida.
///   4. Fade Out → aplica transformações finais no pico do preto → Fade In.
/// 
/// Coloque em um GameObject vazio da cena de destino.
/// A sequência só roda uma vez por sessão (flag consumida no Start).
/// </summary>
public class CinematicSequence : MonoBehaviour
{
    // ── Flag de ativação ─────────────────────────────────────────────
    [Header("Ativação")]
    [Tooltip("Se true, a sequência dispara automaticamente ao carregar a cena.")]
    public bool dispararAoCarregar = false;

    // ── Diálogo 1 ────��───────────────────────────────────────────────
    [Header("1 · Diálogo inicial (imediato ao entrar na cena)")]
    public DialogueAsset dialogoInicial;

    // ── Transformações durante o fade ────────────────────────────────
    [Header("2 · Transformações no pico do fade (tela preta)")]
    [Tooltip("Lista de objetos que serão reposicionados e/ou terão o sprite trocado durante o fade.")]
    public System.Collections.Generic.List<TransformacaoPersonagem> transformacoes;

    // ── Diálogo 2 ────────────────────────────────────────────────────
    [Header("3 · Diálogo final (após o fade)")]
    public DialogueAsset dialogoFinal;

    // ── Transformações finais ─────────────────────────────────────────
    [Header("4 · Transformações finais (fade out → aplica → fade in, após o diálogo final)")]
    [Tooltip("Lista de objetos modificados no pico de um segundo fade APÓS o diálogo final terminar.")]
    public System.Collections.Generic.List<TransformacaoPersonagem> transformacoesFinais;

    [Header("Configuração de Fade")]
    [Tooltip("Velocidade do fade intermediário. Se 0, usa GameManager.fadeSpeed.")]
    [Min(0f)]
    public float velocidadeFade = 0f;

    [Tooltip("Delay em segundos (tempo real) entre o fade in terminar e o diálogo final iniciar.")]
    [Min(0f)]
    public float delayAntesDialogoFinal = 0.1f;

    // ── Dados de transformação ────────────────────────────────────────
    [System.Serializable]
    public class TransformacaoPersonagem
    {
        [Tooltip("O GameObject do personagem / objeto a ser modificado. Se vazio, usa tagDoAlvo para buscar em runtime.")]
        public GameObject alvo;

        [Tooltip("Tag do GameObject a buscar em runtime. Usado quando 'alvo' está vazio ou foi destruído.")]
        public string tagDoAlvo;

        [Header("Posição (opcional)")]
        [Tooltip("Se true, teleporta o alvo para novaPosicaoLocal (posição local) no pico do preto.")]
        public bool alterarPosicao = false;
        public Vector3 novaPosicaoLocal;

        [Header("Rotação (opcional)")]
        [Tooltip("Se true, aplica novaRotacao ao alvo no pico do preto.")]
        public bool alterarRotacao = false;
        public Vector3 novaRotacao;

        [Header("Sprite no pico do fade (opcional — exige SpriteRenderer)")]
        [Tooltip("Se diferente de null, troca o sprite do SpriteRenderer do alvo no pico do preto.")]
        public Sprite novoSprite;

        [Header("Flip do objeto (opcional)")]
        [Tooltip("Se true, altera o flip horizontal do objeto inteiro (via localScale.x) no pico do preto.")]
        public bool alterarFlip = false;
        [Tooltip("Se true, inverte o sinal de localScale.x do objeto (flip para a esquerda). Se false, garante localScale.x positivo (flip para a direita).")]
        public bool flipParaEsquerda = false;

        [Tooltip("Se true, inverte o flip horizontal do objeto inteiro (via localScale.x) ao fim da sequência.")]
        public bool inverterFlipAoFim = false;

        [Header("Ativar/Desativar (opcional)")]
        [Tooltip("Se true, chama SetActive(ativarOuDesativar) no alvo.")]
        public bool alterarAtivacao = false;
        public bool ativarOuDesativar = true;
    }

    // ── Internals ─────────────────────────────────────────────────────
    private bool _jaRodou = false;

    private void Start()
    {
        if (!dispararAoCarregar) return;
        if (_jaRodou) return;
        _jaRodou = true;
        StartCoroutine(SequenciaCoroutine());
    }

    /// <summary>
    /// Pode ser chamado externamente (ex: por DialogueEndEvent ou outro script)
    /// para iniciar a sequência manualmente.
    /// </summary>
    public void Disparar()
    {
        if (_jaRodou) return;
        _jaRodou = true;
        StartCoroutine(SequenciaCoroutine());
    }

    // ── Coroutine principal ───────────────────────────────────────────
    private IEnumerator SequenciaCoroutine()
    {
        // Espera um frame para garantir que todos os Starts já rodaram
        yield return null;
        yield return new WaitForSecondsRealtime(0.05f);

        // Bloqueia input durante toda a cinemática
        if (GameManager.Instance != null)
            GameManager.Instance.inputBloqueado = true;

        // ── Etapa 1: Diálogo inicial ──────────────────────────────
        if (dialogoInicial != null)
        {
            bool dialogoTerminou = false;
            DialogueRunner.Instance.StartDialogue(dialogoInicial, () => dialogoTerminou = true);
            yield return new WaitUntil(() => dialogoTerminou);
        }
        else
        {
            Debug.LogWarning("[CinematicSequence] dialogoInicial não atribuído — pulando etapa 1.");
        }

        // Garante input bloqueado após o diálogo (EndDialogue pode ter liberado)
        if (GameManager.Instance != null)
            GameManager.Instance.inputBloqueado = true;

        // ── Etapa 2: Fade Out ─────────────────────────────────────
        yield return StartCoroutine(FadeOutLocal());

        // ── Etapa 2b: Transformações no pico do preto ─────────────
        AplicarTransformacoes(transformacoes);
        yield return new WaitForSecondsRealtime(0.05f);

        // ── Etapa 2c: Fade In ─────────────────────────────────────
        yield return StartCoroutine(FadeInLocal());

        if (delayAntesDialogoFinal > 0f)
            yield return new WaitForSecondsRealtime(delayAntesDialogoFinal);

        // ── Etapa 3: Diálogo final ────────────────────────────────
        if (dialogoFinal != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.inputBloqueado = false;

            bool dialogoFinalTerminou = false;
            DialogueRunner.Instance.StartDialogue(dialogoFinal, () => dialogoFinalTerminou = true);
            yield return new WaitUntil(() => dialogoFinalTerminou);
        }
        else
        {
            Debug.LogWarning("[CinematicSequence] dialogoFinal não atribuído — pulando etapa 3.");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.inputBloqueado = true;

        // ── Etapa 4: Fade Out → Transformações finais → Fade In ───
        if (transformacoesFinais != null && transformacoesFinais.Count > 0)
        {
            yield return StartCoroutine(FadeOutLocal());

            AplicarTransformacoes(transformacoesFinais);
            yield return new WaitForSecondsRealtime(0.05f);

            yield return StartCoroutine(FadeInLocal());
        }

        if (GameManager.Instance != null)
            GameManager.Instance.inputBloqueado = false;

        Debug.Log("[CinematicSequence] Sequência cinemática concluída.");
    }

    // ── Resolver alvo em runtime ──────────────────────────────────────
    private GameObject ResolverAlvo(TransformacaoPersonagem t)
    {
        // Tenta usar a referência direta primeiro
        if (t.alvo != null)
            return t.alvo;

        // Fallback: busca por tag em runtime
        if (!string.IsNullOrEmpty(t.tagDoAlvo))
        {
            var encontrado = GameObject.FindWithTag(t.tagDoAlvo);
            if (encontrado != null)
                return encontrado;

            Debug.LogWarning($"[CinematicSequence] Nenhum objeto encontrado com tag '{t.tagDoAlvo}'.");
        }

        return null;
    }

    // ── Aplicar transformações ────────────────────────────────────────
    private void AplicarTransformacoes(System.Collections.Generic.List<TransformacaoPersonagem> lista)
    {
        if (lista == null) return;

        for (int i = 0; i < lista.Count; i++)
        {
            var t = lista[i];
            if (t == null) continue;

            var alvo = ResolverAlvo(t);
            if (alvo == null)
            {
                Debug.LogWarning("[CinematicSequence] Alvo não encontrado — transformação ignorada.");
                continue;
            }

            try
            {
                if (t.alterarPosicao)
                    alvo.transform.localPosition = t.novaPosicaoLocal;

                if (t.alterarRotacao)
                    alvo.transform.eulerAngles = t.novaRotacao;

                if (t.novoSprite != null)
                {
                    var sr = alvo.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = t.novoSprite;

                        // Força o Animator a re-entrar no estado atual para que ele
                        // respeite o novo sprite (Write Defaults desligado é necessário).
                    }
                    else
                    {
                        var srFilho = alvo.GetComponentInChildren<SpriteRenderer>();
                        if (srFilho != null)
                            Debug.LogWarning("[CinematicSequence] '" + alvo.name + "' nao tem SpriteRenderer na raiz — aponte o alvo para o filho '" + srFilho.gameObject.name + "'.");
                        else
                            Debug.LogWarning("[CinematicSequence] Nenhum SpriteRenderer encontrado em '" + alvo.name + "' nem nos filhos.");
                    }
                }

                if (t.alterarFlip)
                {
                    Vector3 scale = alvo.transform.localScale;
                    float absX = Mathf.Abs(scale.x);
                    scale.x = t.flipParaEsquerda ? -absX : absX;
                    alvo.transform.localScale = scale;
                }

                if (t.alterarAtivacao)
                    alvo.SetActive(t.ativarOuDesativar);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CinematicSequence] Erro ao aplicar transformacao em '" + alvo.name + "': " + e.Message);
            }
        }
    }

    // ── Fade helpers (usam o fadeImage do GameManager) ────────────────
    private IEnumerator FadeOutLocal()
    {
        var gm = GameManager.Instance;
        if (gm == null) yield break;

        var fadeImage = gm.GetFadeImage();
        if (fadeImage == null) yield break;

        float speed = velocidadeFade > 0f ? velocidadeFade : gm.fadeSpeed;
        fadeImage.gameObject.SetActive(true);

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * speed;
            fadeImage.color = new UnityEngine.Color(0, 0, 0, Mathf.Min(alpha, 1f));
            yield return null;
        }
        fadeImage.color = new UnityEngine.Color(0, 0, 0, 1f);
    }

    private IEnumerator FadeInLocal()
    {
        var gm = GameManager.Instance;
        if (gm == null) yield break;

        var fadeImage = gm.GetFadeImage();
        if (fadeImage == null) yield break;

        float speed = velocidadeFade > 0f ? velocidadeFade : gm.fadeSpeed;

        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.unscaledDeltaTime * speed;
            fadeImage.color = new UnityEngine.Color(0, 0, 0, Mathf.Max(alpha, 0f));
            yield return null;
        }
        fadeImage.color = new UnityEngine.Color(0, 0, 0, 0f);
    }
}