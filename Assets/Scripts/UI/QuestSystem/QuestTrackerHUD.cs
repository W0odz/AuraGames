using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestTrackerHUD : MonoBehaviour
{
    public static QuestTrackerHUD Instance;

    [Header("Referências")]
    public GameObject painel;
    public TextMeshProUGUI textoNomeQuest;
    public TextMeshProUGUI textoObjetivo;

    [Header("Configuração")]
    public float intervaloPolling = 0.25f;
    public float duracaoFade = 0.3f;
    public float pausaRiscado = 1.0f;
    public float tempoAutoOcultar = 5f;

    [System.NonSerialized] private QuestDefinition questAtual;
    [System.NonSerialized] private Coroutine coroutinePolling;
    [System.NonSerialized] private Coroutine coroutineAnimacao;
    [System.NonSerialized] private Coroutine coroutineNome;
    [System.NonSerialized] private Coroutine coroutineAutoOcultar;
    [System.NonSerialized] private Coroutine coroutineFadeIn;
    [System.NonSerialized] private QuestObjective objetivoExibido;
    private bool painelOcultoManualmente = false;
    private bool _transicaoEmAndamento = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (painel != null)
            painel.SetActive(false);
    }

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada += OnQuestIniciada;
            QuestManager.Instance.onQuestCompleta += OnQuestCompleta;
            QuestManager.Instance.onQuestEntregue += OnQuestEntregue;

            var ativas = QuestManager.Instance.GetAllActive();
            if (ativas != null && ativas.Count > 0)
                MostrarQuest(ativas[0]);
        }
        else
        {
            Debug.LogWarning("[QuestTrackerHUD] QuestManager.Instance não encontrado no Start.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;

            if (painel != null && painel.activeSelf)
            {
                IniciarAutoOcultar(imediato: true);
                painelOcultoManualmente = true;
            }
            else if (questAtual != null)
            {
                painelOcultoManualmente = false;
                ReexibirPainel();
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        coroutinePolling = null;
        coroutineAnimacao = null;
        coroutineNome = null;
        coroutineAutoOcultar = null;
        coroutineFadeIn = null;
        objetivoExibido = null;
        _transicaoEmAndamento = false;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada -= OnQuestIniciada;
            QuestManager.Instance.onQuestCompleta -= OnQuestCompleta;
            QuestManager.Instance.onQuestEntregue -= OnQuestEntregue;
        }
    }

    private void OnQuestIniciada(QuestDefinition def)
    {
        MostrarQuest(def);
    }

    private void OnQuestCompleta(QuestDefinition def)
    {
        if (questAtual == null || def == null || questAtual.questId != def.questId) return;

        if (coroutineNome != null)
        {
            StopCoroutine(coroutineNome);
            coroutineNome = null;
        }
        coroutineNome = StartCoroutine(AnimacaoQuestCompleta(def));
    }

    private void OnQuestEntregue(QuestDefinition def)
    {
        if (questAtual != null && def != null && questAtual.questId == def.questId)
        {
            questAtual = null;
            _transicaoEmAndamento = false;
            if (painel != null)
                painel.SetActive(false);
        }
    }

    public void MostrarQuest(QuestDefinition def)
    {
        if (def == null)
        {
            Debug.LogWarning("[QuestTrackerHUD] MostrarQuest chamado com quest nula.");
            return;
        }

        if (questAtual == null || questAtual.questId != def.questId)
            painelOcultoManualmente = false;

        questAtual = def;
        objetivoExibido = ObterPrimeiroObjetivoIncompleto(def);

        if (textoNomeQuest != null)
        {
            textoNomeQuest.text = def.questName;
            SetAlpha(textoNomeQuest, 0f);
        }

        QuestObjective obj = ObterObjetivoAtual(def);
        if (textoObjetivo != null)
        {
            textoObjetivo.text = obj != null ? FormatarObjetivo(obj) : "";
            SetAlpha(textoObjetivo, 0f);
        }

        if (painel != null)
            painel.SetActive(true);

        IniciarPolling();

        if (!painelOcultoManualmente)
        {
            IniciarFadeIn();
            IniciarAutoOcultar(imediato: false);
        }
    }

    private QuestObjective ObterObjetivoAtual(QuestDefinition def)
    {
        if (def == null || def.objetivos == null) return null;
        foreach (var obj in def.objetivos)
        {
            if (obj != null && !obj.EstaCompleto())
                return obj;
        }
        return null;
    }

    private QuestObjective ObterPrimeiroObjetivoIncompleto(QuestDefinition def)
    {
        if (def == null || def.objetivos == null) return null;
        foreach (var obj in def.objetivos)
        {
            if (obj == null || obj.apenasInformativo) continue;
            if (!obj.EstaCompleto()) return obj;
        }
        return null;
    }

    private string FormatarObjetivo(QuestObjective obj)
    {
        if (obj == null) return "";

        bool exibirContagem = (obj.tipo == QuestObjectiveType.CollectItem || obj.tipo == QuestObjectiveType.KillEnemy)
                              && obj.quantidadeNecessaria > 1;

        if (exibirContagem)
            return $"{obj.descricao} ({obj.progressoAtual}/{obj.quantidadeNecessaria})";

        return obj.descricao;
    }

    private void ReexibirPainel()
    {
        if (questAtual == null) return;

        SetAlpha(textoNomeQuest, 0f);
        SetAlpha(textoObjetivo, 0f);

        if (painel != null)
            painel.SetActive(true);

        IniciarFadeIn();
        IniciarAutoOcultar(imediato: false);
        IniciarPolling();
    }

    // ── Fade In ───────────────────────────────────────────────────

    private void IniciarFadeIn()
    {
        if (coroutineFadeIn != null)
        {
            StopCoroutine(coroutineFadeIn);
            coroutineFadeIn = null;
        }
        coroutineFadeIn = StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < duracaoFade)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duracaoFade);
            SetAlpha(textoNomeQuest, alpha);
            SetAlpha(textoObjetivo, alpha);
            yield return null;
        }
        SetAlpha(textoNomeQuest, 1f);
        SetAlpha(textoObjetivo, 1f);
        coroutineFadeIn = null;
    }

    // ── Auto-ocultar ──────────────────────────────────────────────

    private void IniciarAutoOcultar(bool imediato)
    {
        if (coroutineAutoOcultar != null)
        {
            StopCoroutine(coroutineAutoOcultar);
            coroutineAutoOcultar = null;
        }
        coroutineAutoOcultar = StartCoroutine(AutoOcultarCoroutine(imediato));
    }

    private IEnumerator AutoOcultarCoroutine(bool imediato)
    {
        if (!imediato)
            yield return new WaitForSecondsRealtime(tempoAutoOcultar);

        // Aguarda a transição de objetivo terminar antes de fazer fade out
        while (_transicaoEmAndamento)
            yield return null;

        // Fade out — parte do alpha atual (respeita fade in ainda em andamento)
        float alphaInicio = textoNomeQuest != null ? textoNomeQuest.color.a : 1f;
        float elapsed = 0f;

        while (elapsed < duracaoFade)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(alphaInicio, 0f, elapsed / duracaoFade);
            SetAlpha(textoNomeQuest, alpha);
            SetAlpha(textoObjetivo, alpha);
            yield return null;
        }

        SetAlpha(textoNomeQuest, 0f);
        SetAlpha(textoObjetivo, 0f);

        if (painel != null)
            painel.SetActive(false);
    }

    // ── Polling ───────────────────────────────────────────────────

    private void IniciarPolling()
    {
        if (coroutinePolling != null)
        {
            StopCoroutine(coroutinePolling);
            coroutinePolling = null;
        }
        coroutinePolling = StartCoroutine(PollingCoroutine());
    }

    private IEnumerator PollingCoroutine()
    {
        while (painel != null)
        {
            if (questAtual == null) yield break;

            if (objetivoExibido == null)
                yield break;

            if (objetivoExibido.EstaCompleto())
            {
                QuestObjective proximo = ObterProximoObjetivo(questAtual, objetivoExibido);
                if (proximo != null)
                {
                    if (!painel.activeSelf)
                        painel.SetActive(true);

                    painelOcultoManualmente = false;
                    IniciarFadeIn();
                    IniciarAutoOcultar(imediato: false);
                    IniciarAnimacaoTransicao(objetivoExibido, proximo);
                    objetivoExibido = proximo;
                }
                else
                {
                    if (textoObjetivo != null)
                        textoObjetivo.text = $"<voffset=0.15em><s>{objetivoExibido.descricao}</s></voffset>";
                }
                yield break;
            }

            if (textoObjetivo != null)
                textoObjetivo.text = FormatarObjetivo(objetivoExibido);

            yield return new WaitForSecondsRealtime(intervaloPolling);
        }
    }

    private QuestObjective ObterProximoObjetivo(QuestDefinition def, QuestObjective atual)
    {
        if (def == null || def.objetivos == null) return null;

        bool encontrouAtual = false;
        foreach (var obj in def.objetivos)
        {
            if (obj == null) continue;
            if (encontrouAtual && !obj.EstaCompleto())
                return obj;
            if (obj == atual)
                encontrouAtual = true;
        }
        return null;
    }

    // ── Transição de objetivo ─────────────────────────────────────

    private void IniciarAnimacaoTransicao(QuestObjective objetivoConcluido, QuestObjective proximo)
    {
        if (coroutineAnimacao != null)
        {
            StopCoroutine(coroutineAnimacao);
            coroutineAnimacao = null;
        }
        coroutineAnimacao = StartCoroutine(AnimacaoTransicao(objetivoConcluido, proximo));
    }

    private IEnumerator AnimacaoTransicao(QuestObjective objetivoConcluido, QuestObjective proximo)
    {
        if (textoObjetivo == null) yield break;

        _transicaoEmAndamento = true;

        textoObjetivo.text = $"<voffset=0.15em><s>{objetivoConcluido.descricao}</s></voffset>";

        yield return new WaitForSecondsRealtime(pausaRiscado);

        yield return StartCoroutine(FadeTextoObjetivo(1f, 0f));

        textoObjetivo.text = FormatarObjetivo(proximo);

        yield return StartCoroutine(FadeTextoObjetivo(0f, 1f));

        _transicaoEmAndamento = false;

        IniciarPolling();
    }

    // ── Fades individuais ─────────────────────────────────────────

    private IEnumerator FadeTextoObjetivo(float de, float para)
    {
        if (textoObjetivo == null) yield break;

        float elapsed = 0f;
        Color c = textoObjetivo.color;
        c.a = de;
        textoObjetivo.color = c;

        while (elapsed < duracaoFade)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(de, para, elapsed / duracaoFade);
            textoObjetivo.color = c;
            yield return null;
        }

        c.a = para;
        textoObjetivo.color = c;
    }

    private IEnumerator FadeTextoNome(float de, float para)
    {
        if (textoNomeQuest == null) yield break;

        float elapsed = 0f;
        Color c = textoNomeQuest.color;
        c.a = de;
        textoNomeQuest.color = c;

        while (elapsed < duracaoFade)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(de, para, elapsed / duracaoFade);
            textoNomeQuest.color = c;
            yield return null;
        }

        c.a = para;
        textoNomeQuest.color = c;
    }

    private IEnumerator AnimacaoQuestCompleta(QuestDefinition def)
    {
        if (def == null) yield break;

        if (textoNomeQuest != null)
            textoNomeQuest.text = $"<voffset=0.15em><s>{def.questName}</s></voffset>";

        yield return new WaitForSecondsRealtime(pausaRiscado);

        yield return StartCoroutine(FadeTextoNome(1f, 0f));
    }

    // ── Helper ────────────────────────────────────────────────────

    private void SetAlpha(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }
}