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

    [Header("Posição")]
    public bool cantoSuperiorEsquerdo = false;

    [Header("Configuração")]
    public float intervaloPolling = 0.25f;
    public float duracaoFade = 0.3f;
    public float pausaRiscado = 1.0f;

    [System.NonSerialized] private QuestDefinition questAtual;
    [System.NonSerialized] private Coroutine coroutinePolling;
    [System.NonSerialized] private Coroutine coroutineAnimacao;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        AjustarPosicao();

        if (painel != null)
            painel.SetActive(false);
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada += OnQuestIniciada;
            QuestManager.Instance.onQuestCompleta += OnQuestCompleta;
            QuestManager.Instance.onQuestEntregue += OnQuestEntregue;
        }
        else
        {
            Debug.LogWarning("[QuestTrackerHUD] QuestManager.Instance não encontrado no OnEnable.");
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        coroutinePolling = null;
        coroutineAnimacao = null;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada -= OnQuestIniciada;
            QuestManager.Instance.onQuestCompleta -= OnQuestCompleta;
            QuestManager.Instance.onQuestEntregue -= OnQuestEntregue;
        }
    }

    private void AjustarPosicao()
    {
        if (painel == null) return;

        RectTransform rt = painel.GetComponent<RectTransform>();
        if (rt == null) return;

        if (cantoSuperiorEsquerdo)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -20f);
        }
        else
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
        }
    }

    private void OnQuestIniciada(QuestDefinition def)
    {
        MostrarQuest(def);
    }

    private void OnQuestCompleta(QuestDefinition def)
    {
        // Mantém o painel visível até onQuestEntregue
    }

    private void OnQuestEntregue(QuestDefinition def)
    {
        if (questAtual != null && def != null && questAtual.questId == def.questId)
        {
            questAtual = null;
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

        questAtual = def;

        if (textoNomeQuest != null)
            textoNomeQuest.text = def.questName;

        QuestObjective obj = ObterObjetivoAtual(def);
        if (textoObjetivo != null)
        {
            Color c = textoObjetivo.color;
            c.a = 1f;
            textoObjetivo.color = c;
            textoObjetivo.text = obj != null ? FormatarObjetivo(obj) : "";
        }

        if (painel != null)
            painel.SetActive(true);

        IniciarPolling();
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

    private string FormatarObjetivo(QuestObjective obj)
    {
        if (obj == null) return "";

        bool exibirContagem = (obj.tipo == QuestObjectiveType.CollectItem || obj.tipo == QuestObjectiveType.KillEnemy)
                              && obj.quantidadeNecessaria > 1;

        if (exibirContagem)
            return $"{obj.descricao} ({obj.progressoAtual}/{obj.quantidadeNecessaria})";

        return obj.descricao;
    }

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
        while (painel != null && painel.activeSelf)
        {
            if (questAtual == null) yield break;

            QuestObjective obj = ObterObjetivoAtual(questAtual);

            if (obj == null)
            {
                // Todos os objetivos concluídos — mantém o último riscado até onQuestEntregue
                yield break;
            }

            if (obj.EstaCompleto())
            {
                // Objetivo acabou de ser concluído — animar transição para o próximo
                QuestObjective proximo = ObterProximoObjetivo(questAtual, obj);
                if (proximo != null)
                    IniciarAnimacaoTransicao(obj, proximo);
                else
                {
                    // Era o último objetivo — mostrar riscado e aguardar entrega
                    if (textoObjetivo != null)
                        textoObjetivo.text = $"<s>{obj.descricao}</s>";
                }
                yield break;
            }

            // Atualiza contagem em tempo real
            if (textoObjetivo != null)
                textoObjetivo.text = FormatarObjetivo(obj);

            yield return new WaitForSeconds(intervaloPolling);
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

        // 1. Mostrar riscado
        textoObjetivo.text = $"<s>{objetivoConcluido.descricao}</s>";

        // 2. Aguardar pausa
        yield return new WaitForSeconds(pausaRiscado);

        // 3. Fade out
        yield return StartCoroutine(FadeTextoObjetivo(1f, 0f));

        // 4. Trocar para o próximo objetivo (com alpha 0)
        textoObjetivo.text = FormatarObjetivo(proximo);

        // 5. Fade in
        yield return StartCoroutine(FadeTextoObjetivo(0f, 1f));

        // Reiniciar polling para o novo objetivo
        IniciarPolling();
    }

    private IEnumerator FadeTextoObjetivo(float de, float para)
    {
        if (textoObjetivo == null) yield break;

        float elapsed = 0f;
        Color c = textoObjetivo.color;
        c.a = de;
        textoObjetivo.color = c;

        while (elapsed < duracaoFade)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(de, para, elapsed / duracaoFade);
            textoObjetivo.color = c;
            yield return null;
        }

        c.a = para;
        textoObjetivo.color = c;
    }
}
