using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    private Action _onEnd;
    private QuestDefinition questDoDialogo;
    private bool _fecharNoProximoInput = false; // ← NOVO

    public static DialogueRunner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        if (recentlyOpened)
        {
            if (!Input.GetKey(KeyCode.E)) recentlyOpened = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            AdvanceDialogue();
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
        _fecharNoProximoInput = false; // ← NOVO: reset ao iniciar
        dialoguePanel.SetActive(true);
        recentlyOpened = true;

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
        // ← NOVO: se StartQuest foi executado no input anterior, fecha agora
        if (_fecharNoProximoInput)
        {
            _fecharNoProximoInput = false;
            EndDialogue();
            return;
        }

        var currentNode = currentAsset.nodes[currentIndex];
        if (currentNode.acaoDeQuest != DialogueActionType.None)
        {
            DialogueActions.Execute(currentNode.acaoDeQuest, currentNode.questDef);

            // ← NOVO: agenda fechamento no próximo input após StartQuest
            if (currentNode.acaoDeQuest == DialogueActionType.StartQuest)
            {
                _fecharNoProximoInput = true;
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
        _fecharNoProximoInput = false; // ← NOVO: reset ao fechar
        dialoguePanel.SetActive(false);
        currentAsset = null;

        GameManager.Instance.inputBloqueado = false;

        var cb = _onEnd;
        _onEnd = null;
        cb?.Invoke();
        Time.timeScale = 1f;
    }
}