using UnityEngine;
using TMPro;

/// <summary>
/// Representa um slot de quest no QuestLogUI.
/// Chame Atualizar(def) a qualquer momento para atualizar a exibição.
/// </summary>
public class QuestEntryUI : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoTipo;
    public Transform listaObjetivos;
    public GameObject prefabTextoObjetivo;
    public TextMeshProUGUI textoEntrega;

    /// <summary>
    /// Preenche/atualiza o slot com os dados da quest fornecida.
    /// </summary>
    public void Atualizar(QuestDefinition def)
    {
        if (def == null)
        {
            Debug.LogWarning("[QuestEntryUI] Atualizar chamado com QuestDefinition nula.");
            return;
        }

        // Nome da quest
        if (textoNome != null)
            textoNome.text = def.questName;

        // Tag colorida por tipo
        if (textoTipo != null)
        {
            switch (def.questType)
            {
                case QuestType.MainQuest:
                    textoTipo.text = "Principal";
                    textoTipo.color = Color.yellow;
                    break;
                case QuestType.Daily:
                    textoTipo.text = "Diária";
                    textoTipo.color = Color.cyan;
                    break;
                default: // SideQuest
                    textoTipo.text = "Secundária";
                    textoTipo.color = Color.white;
                    break;
            }
        }

        // Reconstrói lista de objetivos
        if (listaObjetivos != null)
        {
            foreach (Transform filho in listaObjetivos)
                Destroy(filho.gameObject);

            if (def.objetivos != null && prefabTextoObjetivo != null)
            {
                foreach (var obj in def.objetivos)
                {
                    GameObject go = Instantiate(prefabTextoObjetivo, listaObjetivos);
                    TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>();
                    if (txt == null)
                    {
                        Debug.LogWarning("[QuestEntryUI] prefabTextoObjetivo não possui TextMeshProUGUI.");
                        continue;
                    }
                    txt.text = FormatarObjetivo(obj);
                }
            }
        }

        // "Pronta para entrega!" — aparece quando quest está Completed mas não entregue
        if (textoEntrega != null)
        {
            bool pronta = QuestManager.Instance != null && QuestManager.Instance.IsCompleted(def.questId);
            textoEntrega.gameObject.SetActive(pronta);
            if (pronta)
            {
                textoEntrega.text = "✔ Pronta para entrega!";
                textoEntrega.color = Color.green;
            }
        }
    }

    private string FormatarObjetivo(QuestObjective obj)
    {
        bool completo = obj.EstaCompleto();

        if (obj.tipo == QuestObjectiveType.Timer)
        {
            if (completo)
                return $"[✓] {obj.descricao}";

            float atual = Mathf.Min(obj.timerAtual, obj.timerNecessario);
            return $"[ ] {obj.descricao} ({atual:F1}s / {obj.timerNecessario:F1}s)";
        }

        // Todos os outros tipos
        if (completo)
            return $"[✓] {obj.descricao}";

        if (obj.quantidadeNecessaria <= 1)
            return $"[ ] {obj.descricao}";

        return $"[ ] {obj.descricao} ({obj.progressoAtual}/{obj.quantidadeNecessaria})";
    }
}
