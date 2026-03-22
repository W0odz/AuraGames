using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // Usado para encontrar os outros slots

public class SaveSlotUI : MonoBehaviour
{
    [Header("Configura��o do Slot")]
    public int slotID;

    [Header("Refer�ncias da UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI loadButtonText;
    public Button loadButton;
    public Button copyButton;
    public Button eraseButton;

    [Header("Refer�ncias Externas")]
    public NameInputPopup namePopup;

    private void Start()
    {
        RefreshUI();

        // Garante que não haja listeners duplicados (caso o Inspector já os tenha)
        loadButton.onClick.RemoveAllListeners();
        eraseButton.onClick.RemoveAllListeners();
        copyButton.onClick.RemoveAllListeners();

        loadButton.onClick.AddListener(OnLoadClicked);
        eraseButton.onClick.AddListener(OnEraseClicked);
        copyButton.onClick.AddListener(OnCopyClicked);
    }

    // Esta fun��o ser� chamada por outros slots quando
    // o estado de "copiar" mudar
    public void RefreshUI()
    {
        GameData data = SaveSystem.LoadGame(slotID);

        if (data != null)
        {
            // --- SE EXISTE UM SAVE ---
            levelText.text = "N�vel: " + data.playerLevel;
            loadButtonText.text = "Carregar";

            SetButtonInteractable(loadButton, true);
            SetButtonInteractable(eraseButton, true);
            SetButtonInteractable(copyButton, true); // Pode copiar um save que existe
        }
        else
        {
            // --- SE O SLOT EST� VAZIO ---
            levelText.text = "N�vel: --";

            // Verifica se h� algo na "�rea de transfer�ncia"
            if (GameManager.dataToCopy != null)
            {
                loadButtonText.text = "Colar"; // "Paste"
            }
            else
            {
                loadButtonText.text = "Jogo Novo";
            }

            SetButtonInteractable(loadButton, true);
            SetButtonInteractable(eraseButton, false);
            SetButtonInteractable(copyButton, false); // N�o pode copiar um slot vazio
        }
    }

    private void SetButtonInteractable(Button btn, bool interactable)
    {
        if (btn == null) return;
        btn.interactable = interactable;
        // Desabilita o raycast quando não interactável para impedir hover visual
        var graphic = btn.targetGraphic;
        if (graphic != null)
            graphic.raycastTarget = interactable;
    }

    // Chamado quando o bot�o "Load / Jogo Novo / Colar" � clicado
    public void OnLoadClicked()
    {
        // 1. Define o slot atual no GameManager
        GameManager.Instance.SetCurrentSlot(slotID);

        // 2. Decide a a��o
        if (SaveSystem.SaveFileExists(slotID))
        {
           // 1. Carrega os dados (isso preenche 'sceneToLoad' no GameManager)
            GameManager.Instance.LoadGame(slotID);
            
            // 2. Carrega a cena correta que estava no save
            string cenaParaCarregar = GameManager.Instance.sceneToLoad;
            
            // Seguran�a: Se por algum motivo estiver vazio, vai pra Exploration
            if (string.IsNullOrEmpty(cenaParaCarregar)) cenaParaCarregar = "Vila_01";

            GameManager.Instance.LoadSceneWithFade(cenaParaCarregar);
        }
        else if (GameManager.dataToCopy != null)
        {
            // --- Colar Jogo (Este slot est� vazio, mas o clipboard n�o) ---
            SaveSystem.SaveGame(GameManager.dataToCopy, slotID);

            // Limpa o clipboard
            GameManager.dataToCopy = null;

            // Atualiza todos os slots
            UpdateAllSlotUIs();
        }
        else
        {
            if (namePopup != null)
            {
                namePopup.OpenPopup(slotID);
            }
            else
            {
                Debug.LogError("ERRO: Arraste o NameInputPanel para o campo 'Name Popup' no Inspector do SaveSlotUI!");
            }
        }
    }

    // Chamado quando o bot�o "Erase" � clicado
    public void OnEraseClicked()
    {
        SaveSystem.EraseGame(slotID);

        // Se est�vamos copiando este slot, limpa o clipboard
        if (GameManager.dataToCopy != null && SaveSystem.LoadGame(slotID) == null)
        {
            GameManager.dataToCopy = null;
        }

        // Atualiza todos os slots
        UpdateAllSlotUIs();
    }

    // Chamado quando o bot�o "Copy" � clicado
    public void OnCopyClicked()
    {
        // Pega os dados deste slot e os coloca no clipboard
        GameData dataToCopy = SaveSystem.LoadGame(slotID);
        if (dataToCopy != null)
        {
            GameManager.dataToCopy = dataToCopy;
            Debug.Log("Slot " + slotID + " copiado!");

            // Atualiza todos os outros slots para mostrar a op��o "Colar"
            UpdateAllSlotUIs();
        }
    }

    // Uma fun��o "helper" que avisa todos os outros slots para se atualizarem
    private void UpdateAllSlotUIs()
    {
        // Encontra todos os scripts SaveSlotUI na cena
        SaveSlotUI[] allSlots = FindObjectsByType<SaveSlotUI>(FindObjectsSortMode.None);
        foreach (SaveSlotUI slot in allSlots)
        {
            slot.RefreshUI();
        }
    }
}