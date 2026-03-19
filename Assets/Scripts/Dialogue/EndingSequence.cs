using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia a sequência de fim de jogo.
/// Coloque na cena da cutscene final.
///
/// Fluxo:
///   1. Espera o DialogueRunner ficar disponível
///   2. Espera o dialogoFinal COMEÇAR
///   3. Espera o dialogoFinal TERMINAR
///   4. Fade out da cena
///   5. Ativa o painel de agradecimento com fade in
///   6. Aguarda duracaoPainel segundos
///   7. Fade out do painel
///   8. Carrega TitleScreen
/// </summary>
public class EndingSequence : MonoBehaviour
{
    [Header("Diálogo Final")]
    [Tooltip("O mesmo DialogueAsset configurado no DialogoPosBatalha do BattleSystem.")]
    public DialogueAsset dialogoFinal;

    [Header("Painel de Agradecimento")]
    [Tooltip("GameObject do painel (deve começar desativado).")]
    public GameObject painelAgradecimento;

    [Tooltip("CanvasGroup do painel para fade suave. Se nulo, aparece instantaneamente.")]
    public CanvasGroup canvasGroupPainel;

    [Tooltip("Segundos que o painel fica visível.")]
    [Min(1f)]
    public float duracaoPainel = 4f;

    [Tooltip("Duração do fade in/out do painel.")]
    [Min(0.1f)]
    public float duracaoFadePainel = 1f;

    private IEnumerator Start()
    {
        if (painelAgradecimento != null)
            painelAgradecimento.SetActive(false);

        // ── Etapa 1: Espera o DialogueRunner existir (máx. 5s) ───────────
        float timeout = 5f;
        while (DialogueRunner.Instance == null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (DialogueRunner.Instance == null)
        {
            Debug.LogError("[EndingSequence] DialogueRunner.Instance não encontrado após 5s!");
            yield break;
        }

        // ── Etapa 2: Espera o dialogoFinal COMEÇAR ────────────────────────
        yield return new WaitUntil(() =>
            DialogueRunner.Instance != null &&
            DialogueRunner.Instance.IsDialogueActive &&
            DialogueRunner.Instance.currentAsset == dialogoFinal
        );

        // ── Etapa 3: Espera o dialogoFinal TERMINAR ───────────────────────
        bool terminou = false;
        System.Action onEnd = () => terminou = true;
        DialogueRunner.Instance.onDialogueEnd += onEnd;

        yield return new WaitUntil(() => terminou);

        // Remove o listener
        if (DialogueRunner.Instance != null)
            DialogueRunner.Instance.onDialogueEnd -= onEnd;

        // ── Etapa 4 em diante: sequência de fim de jogo ───────────────────
        yield return StartCoroutine(SequenciaFimDeJogo());
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
