using System;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class DialogueRunner : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Image leftPortrait;
    public Image rightPortrait;
    public TMP_Text dialogueText;

    public GameObject leftNameBox;
    public TMP_Text leftNameText;
    public GameObject rightNameBox;
    public TMP_Text rightNameText;

    [Header("Cor de destaque / escurecimento")]
    public float alphaEscurecido = 0.4f;

    public DialogueAsset currentAsset;
    private int currentIndex = 0;
    private bool recentlyOpened = false;
    private float recentlyOpenedTime = 0f;       // ← tempo unscaled em que abriu
    private const float recentlyOpenedDelay = 0.15f; // ← delay mínimo antes de aceitar input
    private Action _onEnd;
    private QuestDefinition questDoDialogo;

    public float ultimoFechamentoTime { get; private set; } = -999f;

    public static DialogueRunner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        if (recentlyOpened)
        {
            if (Time.unscaledTime - recentlyOpenedTime >= recentlyOpenedDelay)
                recentlyOpened = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    public bool IsDialogueActive
        => currentAsset != null && dialoguePanel != null && dialoguePanel.activeSelf;

    public void StartDialogue(DialogueAsset asset, QuestDefinition quest, Action onEnd = null)
    {
        questDoDialogo = quest;
        IniciarDialogoInterno(asset, onEnd);
    }

    public void StartDialogue(DialogueAsset asset, Action onEnd = null)
    {
        questDoDialogo = null;
        IniciarDialogoInterno(asset, onEnd);
    }

    void IniciarDialogoInterno(DialogueAsset asset, Action onEnd)
    {
        GameManager.Instance.inputBloqueado = true;
        _onEnd = onEnd;
        currentAsset = asset;
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        recentlyOpened = true;
        recentlyOpenedTime = Time.unscaledTime;

        AplicarPortraitFixo(leftPortrait, asset.portraitEsquerda);
        AplicarPortraitFixo(rightPortrait, asset.portraitDireita);

        Time.timeScale = 0f;
        AvancarParaProximoNoVisivel();
    }

    void AplicarPortraitFixo(Image img, Sprite sprite)
    {
        if (sprite == null)
            img.gameObject.SetActive(false);
        else
        {
            img.sprite = sprite;
            img.gameObject.SetActive(true);
            SetBrilho(img, true);
        }
    }

    void ShowNode()
    {
        var node = currentAsset.nodes[currentIndex];
        dialogueText.text = node.text;

        bool ladoEsquerda = node.speakerSide == DialogueSide.Left;

        if (ladoEsquerda)
        {
            if (node.portrait != null)
            {
                leftPortrait.sprite = node.portrait;
                leftPortrait.gameObject.SetActive(true);
            }
            leftNameBox.SetActive(true);
            leftNameText.text = node.speakerName;
            rightNameBox.SetActive(false);
        }
        else
        {
            if (node.portrait != null)
            {
                rightPortrait.sprite = node.portrait;
                rightPortrait.gameObject.SetActive(true);
            }
            rightNameBox.SetActive(true);
            rightNameText.text = node.speakerName;
            leftNameBox.SetActive(false);
        }

        if (leftPortrait.gameObject.activeSelf)
            SetBrilho(leftPortrait, ladoEsquerda);

        if (rightPortrait.gameObject.activeSelf)
            SetBrilho(rightPortrait, !ladoEsquerda);
    }

    void SetBrilho(Image img, bool ativo)
    {
        var c = img.color;
        float alvo = ativo ? 1f : alphaEscurecido;
        c.r = alvo; c.g = alvo; c.b = alvo; c.a = 1f;
        img.color = c;
    }

    void AdvanceDialogue()
    {
        var currentNode = currentAsset.nodes[currentIndex];

        // Executa a ação de quest do nó atual
        if (currentNode.acaoDeQuest != DialogueActionType.None)
        {
            DialogueActions.Execute(currentNode.acaoDeQuest, currentNode.questDef);

            // Após StartQuest, fecha o diálogo imediatamente
            // O nó Active só deve aparecer na PRÓXIMA interação
            if (currentNode.acaoDeQuest == DialogueActionType.StartQuest)
            {
                EndDialogue();
                return;
            }
        }

        currentIndex++;
        AvancarParaProximoNoVisivel();
    }

    void AvancarParaProximoNoVisivel()
    {

        while (currentIndex < currentAsset.nodes.Length)
        {
            var node = currentAsset.nodes[currentIndex];
            bool visivel = NoEstaVisivel(node);

            if (NoEstaVisivel(currentAsset.nodes[currentIndex]))
            {
                ShowNode();
                return;
            }
            currentIndex++;
        }
        EndDialogue();
    }

    bool NoEstaVisivel(DialogueNode node)
    {
        if (questDoDialogo == null || node.estadoQuest == QuestStateFilter.Sempre)
            return true;

        var qm = QuestManager.Instance;
        if (qm == null) return true;

        string id = questDoDialogo.questId;

        switch (node.estadoQuest)
        {
            case QuestStateFilter.NotStarted:
                return !qm.IsActive(id) && !qm.IsCompleted(id) && !qm.IsTurnedIn(id);
            case QuestStateFilter.Active:
                return qm.IsActive(id);
            case QuestStateFilter.Completed:
                return qm.IsCompleted(id);
            case QuestStateFilter.TurnedIn:
                return qm.IsTurnedIn(id);
            default:
                return true;
        }
    }

    public void EndDialogue()
    {
        ultimoFechamentoTime = Time.unscaledTime;
        dialoguePanel.SetActive(false);
        currentAsset = null;

        GameManager.Instance.inputBloqueado = false;

        var cb = _onEnd;
        _onEnd = null;
        cb?.Invoke();
        Time.timeScale = 1f;
    }
}