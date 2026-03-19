using System.Collections;
using UnityEngine;

public class QuestObjectiveListener : MonoBehaviour
{
    [Header("Quest a observar")]
    public QuestDefinition quest;

    [Tooltip("Índice (0 = primeiro objetivo) do objetivo que deve acionar as ações abaixo.")]
    public int indiceDoObjetivo = 3;

    [Header("GameObjects para desativar ao concluir o objetivo")]
    public GameObject[] desativarAoConcluir;

    [Header("GameObjects para ativar ao concluir o objetivo")]
    public GameObject[] ativarAoConcluir;

    private bool _objetivoConcluido = false;
    private bool _acoesConcluidas = false;

    private void Start()
    {
        // Registra AQUI — depois que todos os Awake/OnEnable já rodaram
        if (QuestManager.Instance != null)
            QuestManager.Instance.onObjetivoConcluido += OnObjetivoConcluido;

        StartCoroutine(InicializarEstado());
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.onObjetivoConcluido -= OnObjetivoConcluido;
    }

    private IEnumerator InicializarEstado()
    {
        yield return null;

        if (quest == null || QuestManager.Instance == null) yield break;
        if (quest.objetivos == null || indiceDoObjetivo >= quest.objetivos.Count) yield break;

        // Guarda se a quest está ativa ou já foi concluída/entregue.
        // Se estiver NotStarted, o objetivo nunca foi iniciado — não desativar nada.
        bool questIniciadaOuConcluida = QuestManager.Instance.IsActive(quest.questId)
                                     || QuestManager.Instance.IsCompleted(quest.questId)
                                     || QuestManager.Instance.IsTurnedIn(quest.questId);

        if (!questIniciadaOuConcluida) yield break;

        if (!quest.objetivos[indiceDoObjetivo].EstaCompleto()) yield break;

        // Reentrou na cena com objetivo já concluído — desativa direto
        _objetivoConcluido = true;
        _acoesConcluidas = true;
        AplicarDesativar();
        AplicarAtivar();
    }

    private void OnObjetivoConcluido(QuestDefinition def, int indice)
    {
        Debug.Log($"[QuestObjectiveListener] Evento recebido — quest={def.questId}, indice={indice}, esperado={indiceDoObjetivo}");

        if (quest == null || def.questId != quest.questId) return;
        if (indice != indiceDoObjetivo) return;

        Debug.Log($"[QuestObjectiveListener] BATEU! Ativando {ativarAoConcluir.Length} objetos."); // ← NOVO
        foreach (var obj in ativarAoConcluir)
            Debug.Log($"[QuestObjectiveListener] → {(obj == null ? "NULL" : obj.name)} = SetActive(true)"); // ← NOVO

        _objetivoConcluido = true;
        AplicarAtivar();
    }

    private void Update()
    {
        if (!_objetivoConcluido || _acoesConcluidas) return;

        Camera cam = Camera.main;
        bool algumAtivo = false;
        bool algumNaCamera = false;

        foreach (var obj in desativarAoConcluir)
        {
            if (obj == null || !obj.activeSelf) continue;
            algumAtivo = true;
            if (cam != null && EstaVisivelNaCamera(obj, cam))
            {
                algumNaCamera = true;
                break;
            }
        }

        if (algumAtivo && !algumNaCamera)
        {
            _acoesConcluidas = true;
            AplicarDesativar();
            Debug.Log("[QuestObjectiveListener] Objetos fora da câmera — desativados.");
        }
    }

    private bool EstaVisivelNaCamera(GameObject obj, Camera cam)
    {
        var renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(cam),
                renderer.bounds
            );

        Vector3 vp = cam.WorldToViewportPoint(obj.transform.position);
        return vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f && vp.z > 0f;
    }

    private void AplicarDesativar()
    {
        foreach (var obj in desativarAoConcluir)
            if (obj != null) obj.SetActive(false);
    }

    private void AplicarAtivar()
    {
        foreach (var obj in ativarAoConcluir)
            if (obj != null) obj.SetActive(true);
    }
}