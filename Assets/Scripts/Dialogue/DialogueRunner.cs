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
    public float alphaEscurecido = 0.4f;  // ← ajusta no Inspector

    public DialogueAsset currentAsset;
    private int currentIndex = 0;
    private bool recentlyOpened = false;
    private Action _onEnd;

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

    public void StartDialogue(DialogueAsset asset, Action onEnd = null)
    {
        _onEnd = onEnd;
        currentAsset = asset;
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        recentlyOpened = true;

        // Aplica retratos fixos do asset (null = oculto)
        AplicarPortraitFixo(leftPortrait, asset.portraitEsquerda);
        AplicarPortraitFixo(rightPortrait, asset.portraitDireita);

        ShowNode();
    }

    void AplicarPortraitFixo(Image img, Sprite sprite)
    {
        if (sprite == null)
        {
            // Não desativa — deixa o ShowNode decidir baseado no node
            img.gameObject.SetActive(false); // começa oculto
        }
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
            // Ativa e atualiza o portrait esquerdo se o node tiver sprite
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
            // Ativa e atualiza o portrait direito se o node tiver sprite
            if (node.portrait != null)
            {
                rightPortrait.sprite = node.portrait;
                rightPortrait.gameObject.SetActive(true);
            }

            rightNameBox.SetActive(true);
            rightNameText.text = node.speakerName;
            leftNameBox.SetActive(false);
        }

        // Escurece quem não está falando
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
        currentIndex++;
        if (currentIndex >= currentAsset.nodes.Length)
        {
            EndDialogue();
            return;
        }
        ShowNode();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentAsset = null;

        var cb = _onEnd;
        _onEnd = null;
        cb?.Invoke();
    }
}