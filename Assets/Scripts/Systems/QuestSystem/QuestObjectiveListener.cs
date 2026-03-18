using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine(InicializarEstado());
    }

    private IEnumerator InicializarEstado()
    {
        yield return null; // espera um frame para o QuestManager inicializar

        // Só restaura estado se o objetivo ESPECÍFICO já estiver concluído ao reentrar na cena
        if (quest == null || QuestManager.Instance == null) yield break;
        if (quest.objetivos == null || indiceDoObjetivo >= quest.objetivos.Count) yield break;
        if (!quest.objetivos[indiceDoObjetivo].EstaCompleto()) yield break;

        // Já estava concluído — desativa direto sem checar câmera
        _objetivoConcluido = true;
        _acoesConcluidas = true;
        AplicarDesativar();
        AplicarAtivar();
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.onObjetivoConcluido += OnObjetivoConcluido;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.onObjetivoConcluido -= OnObjetivoConcluido;
    }

    private void OnObjetivoConcluido(QuestDefinition def, int indice)
    {
        if (quest == null || def.questId != quest.questId) return;
        if (indice != indiceDoObjetivo) return;

        _objetivoConcluido = true;

        // Ativa imediatamente
        AplicarAtivar();
    }

    private void Update()
    {
        if (!_objetivoConcluido || _acoesConcluidas) return;

        Camera cam = Camera.main;

        bool algumAindaNaCamera = false;
        bool algumAtivo = false;

        foreach (var obj in desativarAoConcluir)
        {
            if (obj == null || !obj.activeSelf) continue;

            algumAtivo = true;

            if (cam != null && EstaVisivelNaCamera(obj, cam))
            {
                algumAindaNaCamera = true;
                break;
            }
        }

        // Só desativa se havia pelo menos um objeto ativo E nenhum está mais na câmera
        if (algumAtivo && !algumAindaNaCamera)
        {
            _acoesConcluidas = true;
            AplicarDesativar();
            Debug.Log($"[QuestObjectiveListener] Todos os objetos fora da câmera — desativados.");
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

        Vector3 viewportPos = cam.WorldToViewportPoint(obj.transform.position);
        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
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