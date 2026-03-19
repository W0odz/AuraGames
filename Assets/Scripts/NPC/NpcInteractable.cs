using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NpcInteractable : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueAsset dialogoPadrao;

    [Header("Diálogo único (primeira vez) — ignorado se questVinculada estiver preenchida")]
    public DialogueAsset dialogoUnico;

    [Header("Quest vinculada (opcional)")]
    [Tooltip("Se preenchida, os nós do dialogoPadrao serão filtrados pelo estado da quest. Deixe vazio para comportamento padrão.")]
    public QuestDefinition questVinculada;

    public bool isMerchant = false;
    private bool playerNearby = false;

    // Tempo unscaled da última interação — evita reabrir o diálogo no mesmo frame que fechou
    private float ultimaInteracaoTime = -999f;
    private const float cooldownInteracao = 0.5f;

    private NpcIdentidade _identidade;
    private string _npcId;

    void Awake()
    {
        _identidade = GetComponent<NpcIdentidade>();
        _npcId = (_identidade != null && !string.IsNullOrEmpty(_identidade.npcId))
            ? _identidade.npcId
            : gameObject.name;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }

    void Update()
    {
        if (GameManager.Instance.inputBloqueado) return;
        if (!playerNearby) return;
        if (DialogueRunner.Instance.IsDialogueActive) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        // Cooldown: ignora se acabou de fechar o diálogo neste mesmo frame / instante
        if (Time.unscaledTime - ultimaInteracaoTime < cooldownInteracao) return;

        // Também ignora se o DialogueRunner acabou de fechar o diálogo
        if (Time.unscaledTime - DialogueRunner.Instance.ultimoFechamentoTime < cooldownInteracao) return;

        // Bloqueia interação se a quest vinculada está ativa mas o objetivo atual não é falar com este NPC
        if (questVinculada != null && QuestManager.Instance != null && QuestManager.Instance.IsActive(questVinculada.questId))
        {
            if (!QuestManager.Instance.ObjetivoAtualEhTalkToNpc(questVinculada.questId, _npcId))
                return;
        }

        ultimaInteracaoTime = Time.unscaledTime;
        OnInteract();
    }

    public void OnInteract()
    {
        // Uses the cached NPC ID resolved in Awake()
        string npcId = _npcId;

        // Quest vinculada — verifica se o objetivo atual é TalkToNpc apontando para este NPC
        if (questVinculada != null)
        {
            if (dialogoPadrao == null)
            {
                Debug.LogWarning($"[NpcInteractable] {gameObject.name} tem questVinculada mas dialogoPadrao está vazio.");
                return;
            }

            bool deveUsarDialogoUnico = dialogoUnico != null
                && QuestManager.Instance != null
                && QuestManager.Instance.ObjetivoAtualEhTalkToNpc(questVinculada.questId, npcId);

            if (deveUsarDialogoUnico)
            {
                if (isMerchant)
                {
                    NpcMerchant merchant = GetComponent<NpcMerchant>();
                    DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                    {
                        NotificarQuestManager();
                        if (merchant != null) merchant.OpenMerchantMenu();
                        var endEvent = GetComponent<DialogueEndEvent>();
                        if (endEvent != null) endEvent.ExecutarAcoes();
                    });
                }
                else
                {
                    DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                    {
                        NotificarQuestManager();
                        var endEvent = GetComponent<DialogueEndEvent>();
                        if (endEvent != null) endEvent.ExecutarAcoes();
                    });
                }
            }
            else
            {
                if (isMerchant)
                {
                    NpcMerchant merchant = GetComponent<NpcMerchant>();
                    DialogueRunner.Instance.StartDialogue(dialogoPadrao, questVinculada, () =>
                    {
                        NotificarQuestManager();
                        if (merchant != null) merchant.OpenMerchantMenu();
                        var endEvent = GetComponent<DialogueEndEvent>();
                        if (endEvent != null) endEvent.ExecutarAcoes();
                    });
                }
                else
                {
                    DialogueRunner.Instance.StartDialogue(dialogoPadrao, questVinculada, () =>
                    {
                        NotificarQuestManager();
                        var endEvent = GetComponent<DialogueEndEvent>();
                        if (endEvent != null) endEvent.ExecutarAcoes();
                    });
                }
            }
            return;
        }

        // Resolve se o diálogo único já foi visto — persiste entre cenas via GameManager
        bool jaViuUnico = dialogoUnico == null || GameManager.Instance.DialogoUnicoVisto(npcId);

        // Sem quest vinculada
        if (isMerchant)
        {
            NpcMerchant merchant = GetComponent<NpcMerchant>();
            if (!jaViuUnico)
            {
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    GameManager.Instance.MarcarDialogoUnicoVisto(npcId);
                    NotificarQuestManager();
                    if (merchant != null) merchant.OpenMerchantMenu();
                    var endEvent = GetComponent<DialogueEndEvent>();
                    if (endEvent != null) endEvent.ExecutarAcoes();
                });
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    NotificarQuestManager();
                    if (merchant != null) merchant.OpenMerchantMenu();
                    var endEvent = GetComponent<DialogueEndEvent>();
                    if (endEvent != null) endEvent.ExecutarAcoes();
                });
            }
        }
        else
        {
            if (!jaViuUnico)
            {
                DialogueRunner.Instance.StartDialogue(dialogoUnico, () =>
                {
                    GameManager.Instance.MarcarDialogoUnicoVisto(npcId);
                    NotificarQuestManager();
                    var endEvent = GetComponent<DialogueEndEvent>();
                    if (endEvent != null) endEvent.ExecutarAcoes();
                });
            }
            else
            {
                DialogueRunner.Instance.StartDialogue(dialogoPadrao, () =>
                {
                    NotificarQuestManager();
                    var endEvent = GetComponent<DialogueEndEvent>();
                    if (endEvent != null) endEvent.ExecutarAcoes();
                });
            }
        }
    }

    // Notifica ao final do diálogo, não no início
    private void NotificarQuestManager()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.NotificarConversa(gameObject);
    }
}