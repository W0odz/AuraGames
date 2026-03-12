using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Painel de log de quests.
/// Toggle com a tecla J. Assina os eventos do QuestManager para atualizar automaticamente.
/// </summary>
public class QuestLogUI : MonoBehaviour
{
    public static QuestLogUI Instance;

    [Header("Referências")]
    public GameObject painel;
    public Transform listaContent;
    public GameObject questEntryPrefab;
    public TextMeshProUGUI textoVazio;
    public Button botaoFechar;

    [Header("Configuração")]
    public float refreshInterval = 0.5f;

    private Coroutine _refreshCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painel.SetActive(false);
    }

    void Start()
    {
        if (botaoFechar != null)
            botaoFechar.onClick.AddListener(Fechar);
    }

    void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada += OnQuestChanged;
            QuestManager.Instance.onQuestCompleta += OnQuestChanged;
            QuestManager.Instance.onQuestEntregue += OnQuestChanged;
        }
    }

    void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestIniciada -= OnQuestChanged;
            QuestManager.Instance.onQuestCompleta -= OnQuestChanged;
            QuestManager.Instance.onQuestEntregue -= OnQuestChanged;
        }

        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (painel.activeSelf)
                Fechar();
            else
                Abrir();
        }
    }

    public void Abrir()
    {
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado)
            return;

        painel.SetActive(true);
        Atualizar();

        if (_refreshCoroutine != null)
            StopCoroutine(_refreshCoroutine);
        _refreshCoroutine = StartCoroutine(RefreshPeriodico());
    }

    public void Fechar()
    {
        painel.SetActive(false);

        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }
    }

    public void Atualizar()
    {
        // Destrói entradas anteriores
        foreach (Transform filho in listaContent)
            Destroy(filho.gameObject);

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestLogUI] QuestManager.Instance é null — não foi possível carregar quests.");
            if (textoVazio != null) textoVazio.gameObject.SetActive(true);
            return;
        }

        List<QuestDefinition> ativas = QuestManager.Instance.GetAllActive();

        bool temQuests = ativas != null && ativas.Count > 0;

        if (textoVazio != null)
            textoVazio.gameObject.SetActive(!temQuests);

        if (!temQuests) return;

        foreach (var def in ativas)
        {
            if (questEntryPrefab == null)
            {
                Debug.LogWarning("[QuestLogUI] questEntryPrefab não atribuído.");
                break;
            }

            GameObject go = Instantiate(questEntryPrefab, listaContent);
            QuestEntryUI entry = go.GetComponent<QuestEntryUI>();
            if (entry == null)
            {
                Debug.LogWarning("[QuestLogUI] questEntryPrefab não possui QuestEntryUI.");
                continue;
            }
            entry.Atualizar(def);
        }
    }

    private IEnumerator RefreshPeriodico()
    {
        WaitForSeconds espera = new WaitForSeconds(refreshInterval);
        while (true)
        {
            yield return espera;
            if (painel.activeSelf)
                Atualizar();
        }
    }

    private void OnQuestChanged(QuestDefinition def)
    {
        if (painel.activeSelf)
            Atualizar();
    }
}
