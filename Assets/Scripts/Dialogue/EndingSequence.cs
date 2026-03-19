using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia a sequência de fim de jogo.
/// Coloque em um GameObject da cena onde o diálogo final acontece.
///
/// Fluxo:
///   1. Aguarda o diálogo final terminar
///   2. Fade out da cena
///   3. Ativa o painel de agradecimento com fade in
///   4. Aguarda <see cref="duracaoPainel"/> segundos
///   5. Fade out do painel
///   6. Carrega TitleScreen com fade in
/// </summary>
public class EndingSequence : MonoBehaviour
{
    [Header("Diálogo Final")]
    [Tooltip("DialogueAsset do diálogo que, ao terminar, dispara a sequência de fim de jogo.")]
    public DialogueAsset dialogoFinal;

    [Header("Painel de Agradecimento")]
    [Tooltip("GameObject do painel de agradecimento (deve começar desativado).")]
    public GameObject painelAgradecimento;

    [Tooltip("CanvasGroup do painel de agradecimento, usado para o fade. Se nulo, o painel aparece instantaneamente.")]
    public CanvasGroup canvasGroupPainel;

    [Tooltip("Segundos que o painel fica visível antes de ir para a tela de título.")]
    [Min(1f)]
    public float duracaoPainel = 4f;

    [Tooltip("Duração do fade in/out do painel de agradecimento.")]
    [Min(0.1f)]
    public float duracaoFadePainel = 1f;

    private void Start()
    {
        if (painelAgradecimento != null)
            painelAgradecimento.SetActive(false);

        // Registra o callback no DialogueRunner para saber quando o diálogo terminou
        if (DialogueRunner.Instance != null)
            DialogueRunner.Instance.onDialogueEnd += OnDialogoFinalTerminou;
        else
            Debug.LogWarning("[EndingSequence] DialogueRunner.Instance não encontrado.");
    }

    private void OnDestroy()
    {
        // Limpa o callback ao destruir o objeto
        if (DialogueRunner.Instance != null)
            DialogueRunner.Instance.onDialogueEnd -= OnDialogoFinalTerminou;
    }

    private void OnDialogoFinalTerminou()
    {
        // Só dispara se o diálogo que terminou for o dialogoFinal configurado
        if (DialogueRunner.Instance != null &&
            DialogueRunner.Instance.currentAsset == dialogoFinal)
            return; // ainda está rodando, não terminou

        // Remove o listener imediatamente para não disparar múltiplas vezes
        if (DialogueRunner.Instance != null)
            DialogueRunner.Instance.onDialogueEnd -= OnDialogoFinalTerminou;

        StartCoroutine(SequenciaFimDeJogo());
    }

    private IEnumerator SequenciaFimDeJogo()
    {
        // Garante que o input está bloqueado durante toda a sequência
        if (GameManager.Instance != null)
            GameManager.Instance.inputBloqueado = true;

        // ── Etapa 1: Fade out da cena ────────────────────────────────────
        // Usa o FadeImage do GameManager via FadeComAcao (fade out → ação → NÃO faz fade in)
        // Precisamos apenas do fade out, então usamos a coroutine diretamente
        var fadeImage = GameManager.Instance?.GetFadeImage();
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float alpha = 0f;
            float fadeSpeed = 1.5f; // mesma velocidade padrão do GameManager
            while (alpha < 1f)
            {
                alpha += Time.unscaledDeltaTime * fadeSpeed;
                fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 1f);
        }

        // ── Etapa 2: Ativa o painel de agradecimento ─────────────────────
        if (painelAgradecimento != null)
        {
            painelAgradecimento.SetActive(true);

            if (canvasGroupPainel != null)
                canvasGroupPainel.alpha = 0f;
        }

        // ── Etapa 3: Fade in — esconde o preto e revela o painel ─────────
        if (fadeImage != null)
        {
            float alpha = 1f;
            float fadeSpeed = 1.5f;
            while (alpha > 0f)
            {
                alpha -= Time.unscaledDeltaTime * fadeSpeed;
                fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 0f);
            fadeImage.gameObject.SetActive(false);
        }

        // Fade in do CanvasGroup do painel (se houver)
        if (canvasGroupPainel != null)
        {
            float t = 0f;
            while (t < duracaoFadePainel)
            {
                t += Time.unscaledDeltaTime;
                canvasGroupPainel.alpha = Mathf.Clamp01(t / duracaoFadePainel);
                yield return null;
            }
            canvasGroupPainel.alpha = 1f;
        }

        // ── Etapa 4: Aguarda o tempo de exibição ─────────────────────────
        yield return new WaitForSecondsRealtime(duracaoPainel);

        // ── Etapa 5: Fade out do painel ───────────────────────────────────
        if (canvasGroupPainel != null)
        {
            float t = 0f;
            while (t < duracaoFadePainel)
            {
                t += Time.unscaledDeltaTime;
                canvasGroupPainel.alpha = Mathf.Clamp01(1f - (t / duracaoFadePainel));
                yield return null;
            }
            canvasGroupPainel.alpha = 0f;
        }

        // ── Etapa 6: Vai para TitleScreen ────────────────────────────────
        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithFade("TitleScreen");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
    }
}
