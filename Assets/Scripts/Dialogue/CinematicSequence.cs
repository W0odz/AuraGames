using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquestra uma sequência cinemática scripted dentro de uma cena:
/// 
///   1. Diálogo imediato ao entrar na cena (disparado automaticamente no Start).
///   2. Fade Out → reposiciona / troca sprites de personagens no pico do preto → Fade In.
///   3. Diálogo final logo em seguida.
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

    // ── Diálogo 1 ────────────────────────────────────────────────────
    [Header("1 · Diálogo inicial (imediato ao entrar na cena)")]
    public DialogueAsset dialogoInicial;

    // ── Transformações durante o fade ────────────────────────────────
    [Header("2 · Transformações no pico do fade (tela preta)")]
    [Tooltip("Lista de objetos que serão reposicionados e/ou terão o sprite trocado durante o fade.")]
    public System.Collections.Generic.List<TransformacaoPersonagem> transformacoes;

    // ── Diálogo 2 ────────────────────────────────────────────────────
    [Header("3 · Diálogo final (após o fade)")]
    public DialogueAsset dialogoFinal;

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
        [Tooltip("O GameObject do personagem / objeto a ser modificado.")]
        public GameObject alvo;

        [Header("Posição (opcional)")]
        [Tooltip("Se true, teleporta o alvo para novaPosicaoLocal (posição local) no pico do preto.")]
        public bool alterarPosicao = false;
        public Vector3 novaPosicaoLocal;

        [Header("Rotação (opcional)")]
        [Tooltip("Se true, aplica novaRotacao ao alvo no pico do preto.")]
        public bool alterarRotacao = false;
        public Vector3 novaRotacao;

        [Header("Sprite (opcional — exige SpriteRenderer)")]
        [Tooltip("Se diferente de null, troca o sprite do SpriteRenderer do alvo durante a cinemática.")]
        public Sprite novoSprite;

        [Tooltip("Sprite para restaurar ao fim da sequência. Se vazio, usa o sprite capturado automaticamente antes da troca.")]
        public Sprite spriteInicial; // fallback serializado

        [Tooltip("Se true, restaura o sprite original do personagem ao fim da sequência.")]
        public bool restaurarSpriteAoFim = true;

        [Header("Flip (opcional — exige SpriteRenderer)")]
        [Tooltip("Se true, altera o flipX do SpriteRenderer do alvo no pico do preto.")]
        public bool alterarFlipX = false;
        public bool novoFlipX = false;

        [Tooltip("Se true, inverte o flipX atual ao fim da sequência (corrige orientação ao voltar para cena normal).")]
        public bool inverterFlipXAoFim = false;

        [Header("Ativar/Desativar (opcional)")]
        [Tooltip("Se true, chama SetActive(ativarOuDesativar) no alvo.")]
        public bool alterarAtivacao = false;
        public bool ativarOuDesativar = true;

        // Cache em runtime — preenchido automaticamente antes da troca
        [System.NonSerialized] public Sprite spriteOriginal;
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
        AplicarTransformacoes();
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
            GameManager.Instance.inputBloqueado = false;

        RestaurarSprites();

        Debug.Log("[CinematicSequence] Sequência cinemática concluída.");
    }

    // ── Aplicar transformações ────────────────────────────────────────
    private void AplicarTransformacoes()
    {
        if (transformacoes == null) return;

        foreach (var t in transformacoes)
        {
            if (t.alvo == null) continue;

            if (t.alterarPosicao)
                t.alvo.transform.localPosition = t.novaPosicaoLocal;

            if (t.alterarRotacao)
                t.alvo.transform.eulerAngles = t.novaRotacao;

            if (t.novoSprite != null)
            {
                var sr = t.alvo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (t.spriteOriginal == null)
                        t.spriteOriginal = sr.sprite;
                    sr.sprite = t.novoSprite;
                }
                else
                    Debug.LogWarning($"[CinematicSequence] {t.alvo.name} não tem SpriteRenderer — sprite ignorado.");
            }

            if (t.alterarFlipX)
            {
                var sr = t.alvo.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.flipX = t.novoFlipX;
            }

            if (t.alterarAtivacao)
                t.alvo.SetActive(t.ativarOuDesativar);
        }
    }

    // ── Restaurar sprites ─────────────────────────────────────────────
    private void RestaurarSprites()
    {
        if (transformacoes == null) return;

        foreach (var t in transformacoes)
        {
            if (t.alvo == null) continue;

            var sr = t.alvo.GetComponent<SpriteRenderer>();

            // Restaurar sprite
            if (t.novoSprite != null && t.restaurarSpriteAoFim)
            {
                Sprite spriteParaRestaurar = t.spriteOriginal ?? t.spriteInicial;
                if (sr != null && spriteParaRestaurar != null)
                    sr.sprite = spriteParaRestaurar;
            }

            // Inverter flipX ao fim (para corrigir orientação)
            if (t.inverterFlipXAoFim && sr != null)
                sr.flipX = !sr.flipX;
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