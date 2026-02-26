using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueRunner : MonoBehaviour
{
    public Canvas dialogueCanvas;
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

    void Update()
    {
        if (!dialogueCanvas.gameObject.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.E))
            AdvanceDialogue();
    }

    public void StartDialogue(DialogueAsset asset)
    {
        currentAsset = asset;
        currentIndex = 0;
        dialogueCanvas.gameObject.SetActive(true);
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
        dialogueCanvas.gameObject.SetActive(false);
    }
}