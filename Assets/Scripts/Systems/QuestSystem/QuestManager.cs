using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<string, QuestState> questStates = new();
    private Dictionary<string, QuestDefinition> questDefs = new();

    public System.Action<QuestDefinition> onQuestIniciada;
    public System.Action<QuestDefinition> onQuestCompleta;
    public System.Action<QuestDefinition> onQuestEntregue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Avança timers e verifica objetivos EnterBattle não têm tick — apenas Timer precisa de Update
        foreach (var kvp in new Dictionary<string, QuestState>(questStates))
        {
            if (kvp.Value != QuestState.Active) continue;
            if (!questDefs.TryGetValue(kvp.Key, out var def)) continue;
            if (def.objetivos == null) continue;

            bool algumMudou = false;
            foreach (var obj in def.objetivos)
            {
                if (obj.tipo != QuestObjectiveType.Timer) continue;
                if (obj.EstaCompleto()) continue;

                obj.timerAtual += Time.deltaTime;
                algumMudou = true;
            }

            if (algumMudou)
                VerificarConclusao(kvp.Key, def);
        }
    }

    /// <summary>Inicia uma quest pelo ScriptableObject. Verifica pré-requisitos.</summary>
    public void StartQuest(QuestDefinition def)
    {
        if (def == null) return;
        if (questStates.TryGetValue(def.questId, out var existing) && existing != QuestState.NotStarted)
            return;

        // Verifica pré-requisitos
        if (def.questsNecessarias != null)
        {
            foreach (var prereq in def.questsNecessarias)
            {
                if (prereq == null) continue;
                if (!IsTurnedIn(prereq.questId))
                {
                    Debug.LogWarning($"[QuestManager] Pré-requisito não atendido: {prereq.questId}");
                    return;
                }
            }
        }

        // Registra a quest
        questStates[def.questId] = QuestState.Active;
        questDefs[def.questId] = def;

        // Reseta progresso dos objetivos
        if (def.objetivos != null)
        {
            foreach (var obj in def.objetivos)
            {
                obj.progressoAtual = 0;
                obj.timerAtual = 0f;
            }
        }

        onQuestIniciada?.Invoke(def);
        Debug.Log($"[QuestManager] Quest iniciada: {def.questName}");
    }

    /// <summary>Overload para manter compatibilidade com chamadas que usam string.</summary>
    public void StartQuest(string questId)
    {
        if (questDefs.TryGetValue(questId, out var def))
            StartQuest(def);
        else
            Debug.LogWarning($"[QuestManager] Quest com ID '{questId}' não registrada. Use StartQuest(QuestDefinition) primeiro.");
    }

    public void NotificarColetaItem(DadosItem item, int quantidade)
    {
        foreach (var kvp in questStates)
        {
            if (kvp.Value != QuestState.Active) continue;
            if (!questDefs.TryGetValue(kvp.Key, out var def)) continue;
            if (def.objetivos == null) continue;

            bool algumMudou = false;
            foreach (var obj in def.objetivos)
            {
                if (obj.tipo != QuestObjectiveType.CollectItem) continue;
                if (obj.itemAlvo != item) continue;
                if (obj.EstaCompleto()) continue;

                obj.progressoAtual = Mathf.Min(obj.progressoAtual + quantidade, obj.quantidadeNecessaria);
                algumMudou = true;
            }

            if (algumMudou)
                VerificarConclusao(kvp.Key, def);
        }
    }

    public void NotificarMorteInimigo(string enemyId)
    {
        foreach (var kvp in questStates)
        {
            if (kvp.Value != QuestState.Active) continue;
            if (!questDefs.TryGetValue(kvp.Key, out var def)) continue;
            if (def.objetivos == null) continue;

            bool algumMudou = false;
            foreach (var obj in def.objetivos)
            {
                if (obj.tipo != QuestObjectiveType.KillEnemy) continue;
                if (obj.enemyId != enemyId) continue;
                if (obj.EstaCompleto()) continue;

                obj.progressoAtual = Mathf.Min(obj.progressoAtual + 1, obj.quantidadeNecessaria);
                algumMudou = true;
            }

            if (algumMudou)
                VerificarConclusao(kvp.Key, def);
        }
    }

    public void NotificarConversa(string npcName)
    {
        foreach (var kvp in questStates)
        {
            if (kvp.Value != QuestState.Active) continue;
            if (!questDefs.TryGetValue(kvp.Key, out var def)) continue;
            if (def.objetivos == null) continue;

            bool algumMudou = false;
            foreach (var obj in def.objetivos)
            {
                if (obj.tipo != QuestObjectiveType.TalkToNpc) continue;
                if (obj.npcName != npcName) continue;
                if (obj.EstaCompleto()) continue;

                obj.progressoAtual = Mathf.Max(1, obj.quantidadeNecessaria);
                algumMudou = true;
            }

            if (algumMudou)
                VerificarConclusao(kvp.Key, def);
        }
    }

    /// <summary>
    /// Chamado pelo BattleSystem no início do SetupBattle.
    /// Completa objetivos do tipo EnterBattle cujo battleEnemyId bate com o inimigo atual.
    /// </summary>
    public void NotificarInicioCombate(string enemyId)
    {
        foreach (var kvp in questStates)
        {
            if (kvp.Value != QuestState.Active) continue;
            if (!questDefs.TryGetValue(kvp.Key, out var def)) continue;
            if (def.objetivos == null) continue;

            bool algumMudou = false;
            foreach (var obj in def.objetivos)
            {
                if (obj.tipo != QuestObjectiveType.EnterBattle) continue;
                if (obj.battleEnemyId != enemyId) continue;
                if (obj.EstaCompleto()) continue;

                obj.progressoAtual = 1;
                algumMudou = true;
            }

            if (algumMudou)
                VerificarConclusao(kvp.Key, def);
        }
    }

    private void VerificarConclusao(string questId, QuestDefinition def)
    {
        if (def.objetivos == null || def.objetivos.Count == 0) return;

        foreach (var obj in def.objetivos)
        {
            if (!obj.EstaCompleto()) return;
        }

        CompleteQuest(questId);
    }

    public void CompleteQuest(string questId)
    {
        if (!questStates.TryGetValue(questId, out var state) || state != QuestState.Active)
            return;

        questStates[questId] = QuestState.Completed;

        if (questDefs.TryGetValue(questId, out var def))
        {
            onQuestCompleta?.Invoke(def);
            Debug.Log($"[QuestManager] Quest completada: {def.questName}");
        }
    }

    public void TurnInQuest(string questId)
    {
        if (!questStates.TryGetValue(questId, out var state) || state != QuestState.Completed)
            return;

        questStates[questId] = QuestState.TurnedIn;

        if (questDefs.TryGetValue(questId, out var def))
        {
            if (PlayerUnit.Instance != null)
                PlayerUnit.Instance.AdicionarXP(def.recompensaXP);
            else
                Debug.LogWarning("[QuestManager] PlayerUnit.Instance é null — XP não entregue.");

            onQuestEntregue?.Invoke(def);
            Debug.Log($"[QuestManager] Quest entregue: {def.questName}. +{def.recompensaXP} XP");
        }
    }

    public bool IsActive(string questId) =>
        questStates.TryGetValue(questId, out var s) && s == QuestState.Active;

    public bool IsCompleted(string questId) =>
        questStates.TryGetValue(questId, out var s) && s == QuestState.Completed;

    public bool IsTurnedIn(string questId) =>
        questStates.TryGetValue(questId, out var s) && s == QuestState.TurnedIn;

    public QuestDefinition GetQuestDef(string questId) =>
        questDefs.TryGetValue(questId, out var def) ? def : null;

    public List<QuestDefinition> GetAllActive()
    {
        var result = new List<QuestDefinition>();
        foreach (var kvp in questStates)
        {
            if (kvp.Value == QuestState.Active && questDefs.TryGetValue(kvp.Key, out var def))
                result.Add(def);
        }
        return result;
    }
}

public enum QuestState { NotStarted, Active, Completed, TurnedIn }
public enum QuestObjectiveType { CollectItem, DeliverItem, KillEnemy, TalkToNpc, Timer, EnterBattle }