using UnityEngine;

public class PermanentRemoval : MonoBehaviour
{
    [Header("Quest que controla a remoção")]
    public QuestDefinition quest;

    [Tooltip("Índice do objetivo que, ao ser concluído, remove este personagem permanentemente.")]
    public int indiceDoObjetivo = 0;

    private void Awake()
    {
        if (quest == null) return;
        if (QuestManager.Instance == null) return;
        if (quest.objetivos == null || indiceDoObjetivo >= quest.objetivos.Count) return;

        if (quest.objetivos[indiceDoObjetivo].EstaCompleto())
            gameObject.SetActive(false);
    }
}