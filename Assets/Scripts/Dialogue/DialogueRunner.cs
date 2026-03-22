using System;
using TMPro;
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

    [Header("Fundo do painel")]
    [Tooltip("Image do Canvas usada como fundo. Ativada quando o DialogueAsset pedir.")]
    public Image fundoImage;

    [Header("Cor de destaque / escurecimento")]
    public float alphaEscurecido = 0.4f;

    [Header("Indicador de botão (canto inferior direito do painel de diálogo)")]
    [Tooltip("GameObject com o ícone do Mouse2 — ativado ao abrir o diálogo, desativado ao fechar.")]
    public GameObject indicadorAvancar;

    public DialogueAsset currentAsset;
    private int currentIndex = 0;
    private bool recentlyOpened = false;
    private float recentlyOpenedTime = 0f;
    private const float recentlyOpenedDelay = 0.15f;
    private bool _eSeguroAnterior = false;
    private Action _onEnd;
    private QuestDefinition questDoDialogo;

    public float ultimoFechamentoTime { get; private set; } = -999f;

    /// <summary>
    /// Disparado ao fim de qualquer diálogo. Usado pelo DialogueEndEvent para executar ações pós-diálogo.
    /// </summary>
    public System.Action onDialogueEnd;

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
            {
                recentlyOpened = false;
                _eSeguroAnterior = false;
            }
            return;
        }

        // Botão de avanço trocado de KeyCode.E para Mouse1 (botão direito do mouse)
        if (Input.GetMouseButtonDown(1))
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

    /// <summary>
    /// Inicia o diálogo imediatamente, sem fazer FadeComAcao.
    /// Use quando a tela já estiver preta (ex: durante FadeToSceneCoroutine).
    /// </summary>
    public void StartDialogueImediato(DialogueAsset asset, Action onEnd = null)
    {
        if (asset == null)
        {
            Debug.LogWarning("[DialogueRunner] StartDialogueImediato chamado com asset null.");
            onEnd?.Invoke();
            return;
        }
        questDoDialogo = null;
        AbrirPainel(asset, onEnd);
    }

    void IniciarDialogoInterno(DialogueAsset asset, Action onEnd)
    {
        if (asset.fundoPainel)
        {
            // Bloqueia input imediatamente e faz fade antes de abrir
            GameManager.Instance.inputBloqueado = true;
            GameManager.Instance.FadeComAcao(() => AbrirPainel(asset, onEnd));
        }
        else
        {
            AbrirPainel(asset, onEnd);
        }
    }

    void AbrirPainel(DialogueAsset asset, Action onEnd)
    {
        GameManager.Instance.inputBloqueado = true;
        _onEnd = onEnd;
        currentAsset = asset;
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        recentlyOpened = true;
        recentlyOpenedTime = Time.unscaledTime;
        _eSeguroAnterior = true;

        // Ativa o fundo SOMENTE se o DialogueAsset tiver um sprite de fundo explícito
        if (fundoImage != null)
        {
            if (asset.fundoPainel != null)
            {
                fundoImage.sprite = asset.fundoPainel;
                fundoImage.gameObject.SetActive(true);
            }
            else
            {
                fundoImage.gameObject.SetActive(false);
            }
        }

        AplicarPortraitFixo(leftPortrait, asset.portraitEsquerda);
        AplicarPortraitFixo(rightPortrait, asset.portraitDireita);

        Time.timeScale = 0f;
        if (indicadorAvancar != null) indicadorAvancar.SetActive(true);
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

        if (currentNode.acaoDeQuest != DialogueActionType.None)
        {
            DialogueActions.Execute(currentNode.acaoDeQuest, currentNode.questDef);

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
        var assetFinal = currentAsset;
        var cbFinal = _onEnd;

        ultimoFechamentoTime = Time.unscaledTime;
        currentAsset = null;
        _onEnd = null;

        // Se há cena destino pendente no GameManager, não faz FadeComAcao aqui —
        // o FadeToSceneCoroutine já controla o fade de saída antes de trocar de cena
        bool temCenaDestino = !string.IsNullOrEmpty(GameManager.Instance?.cenaDestinoPendente);

        if (assetFinal != null && assetFinal.fundoPainel != null && !temCenaDestino)
        {
            Time.timeScale = 1f;
            GameManager.Instance.FadeComAcao(() =>
            {
                dialoguePanel.SetActive(false);
                if (indicadorAvancar != null) indicadorAvancar.SetActive(false);
                if (fundoImage != null)
                    fundoImage.gameObject.SetActive(false);

                GameManager.Instance.inputBloqueado = false;
                cbFinal?.Invoke();
                ConcederRecompensas(assetFinal);
                onDialogueEnd?.Invoke();
                QuestManager.Instance?.NotificarFimDialogo(assetFinal);
            });
        }
        else
        {
            dialoguePanel.SetActive(false);
            if (indicadorAvancar != null) indicadorAvancar.SetActive(false);
            if (fundoImage != null)
                fundoImage.gameObject.SetActive(false);

            GameManager.Instance.inputBloqueado = false;
            cbFinal?.Invoke();
            ConcederRecompensas(assetFinal);
            onDialogueEnd?.Invoke();
            QuestManager.Instance?.NotificarFimDialogo(assetFinal);
            Time.timeScale = 1f;
        }
    }

    private void ConcederRecompensas(DialogueAsset asset)
    {
        if (asset == null || asset.recompensas == null || asset.recompensas.Length == 0) return;
        if (InventoryManager.Instance == null) return;

        foreach (var recompensa in asset.recompensas)
        {
            if (recompensa.item == null) continue;

            switch (recompensa.tipo)
            {
                case RecompensaDialogo.TipoRecompensa.ConcederItem:
                    InventoryManager.Instance.AdicionarItem(recompensa.item, recompensa.quantidade);
                    Debug.Log($"[DialogueRunner] Recompensa concedida: {recompensa.quantidade}x {recompensa.item.nomeItem}");
                    break;

                case RecompensaDialogo.TipoRecompensa.EquiparItem:
                    InventoryManager.Instance.AdicionarItem(recompensa.item, 1);
                    if (EquipmentManager.Instance != null)
                    {
                        EquipmentManager.Instance.Equip(recompensa.item);
                        Debug.Log($"[DialogueRunner] Item equipado via diálogo: {recompensa.item.nomeItem}");
                    }
                    break;
            }
        }
    }
}