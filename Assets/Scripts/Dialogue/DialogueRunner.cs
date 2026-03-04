using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueRunner : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Image leftPortrait;
    public Image rightPortrait;
    public TMP_Text dialogueText;

    // Nome do personagem (um para esquerda, um para direita)
    public GameObject leftNameBox;
    public TMP_Text leftNameText;
    public GameObject rightNameBox;
    public TMP_Text rightNameText;

    public DialogueAsset currentAsset;
    private int currentIndex = 0;

    private bool recentlyOpened = false;

    public static DialogueRunner Instance { get; private set; }

    private void Awake()
    {
        // Garante que só exista um runner
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // (Opcional: DontDestroyOnLoad(gameObject); se quiser persistência entre cenas)
    }

    void Update()
    {
        if (!IsDialogueActive)
            return;

        // Bloqueio — espera o E ser solto antes de aceitar outro avanço
        if (recentlyOpened)
        {
            // Só libera quando o E está solto após abrir
            if (!Input.GetKey(KeyCode.E))
                recentlyOpened = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    public bool IsDialogueActive
    {
        get { return currentAsset != null && dialoguePanel != null && dialoguePanel.gameObject.activeSelf; }
    }

    public void StartDialogue(DialogueAsset asset)
    {
        currentAsset = asset;
        currentIndex = 0;
        dialoguePanel.gameObject.SetActive(true);
        recentlyOpened = true;
        ShowNode();
    }

    void ShowNode()
    {
        var node = currentAsset.nodes[currentIndex];
        dialogueText.text = node.text;

        if (node.speakerSide == DialogueSide.Left)
        {
            leftPortrait.sprite = node.portrait;
            leftPortrait.gameObject.SetActive(true);
            rightPortrait.gameObject.SetActive(false);

            leftNameBox.SetActive(true);
            leftNameText.text = node.speakerName;
            rightNameBox.SetActive(false);
        }
        else // Right
        {
            rightPortrait.sprite = node.portrait;
            rightPortrait.gameObject.SetActive(true);
            leftPortrait.gameObject.SetActive(false);

            rightNameBox.SetActive(true);
            rightNameText.text = node.speakerName;
            leftNameBox.SetActive(false);
        }
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
        dialoguePanel.gameObject.SetActive(false);
    }
}