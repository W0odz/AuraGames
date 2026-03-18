using UnityEngine;

/// <summary>
/// Escuta a conclusão de um objetivo específico de uma quest e executa ações na cena
/// (ativar/desativar GameObjects) sem polling.
/// Coloque este componente em qualquer GameObject da cena.
/// </summary>
public class QuestObjectiveListener : MonoBehaviour
{
    [Header("Quest a observar")]
    [Tooltip("A QuestDefinition que contém o objetivo a ser monitorado.")]
    public QuestDefinition quest;

    [Tooltip("Índice (0 = primeiro objetivo) do objetivo que deve acionar as ações abaixo.")]
    public int indiceDoObjetivo = 3; // 4° objetivo = índice 3

    [Header("GameObjects para desativar ao concluir o objetivo")]
    public GameObject[] desativarAoConcluir;

    [Header("GameObjects para ativar ao concluir o objetivo")]
    public GameObject[] ativarAoConcluir;

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

        foreach (var obj in desativarAoConcluir)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in ativarAoConcluir)
            if (obj != null) obj.SetActive(true);

        Debug.Log($"[QuestObjectiveListener] Objetivo {indice} de '{def.questName}' concluído — ações executadas.");
    }
}
