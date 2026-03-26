using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Configuração do Slot")]
    public int slotID;

    [Header("Referências da UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI loadButtonText;
    public Button loadButton;
    public Button copyButton;
    public Button eraseButton;

    private void Start()
    {
        RefreshUI();

        loadButton.onClick.RemoveAllListeners();
        eraseButton.onClick.RemoveAllListeners();
        copyButton.onClick.RemoveAllListeners();

        loadButton.onClick.AddListener(OnLoadClicked);
        eraseButton.onClick.AddListener(OnEraseClicked);
        copyButton.onClick.AddListener(OnCopyClicked);
    }

    public void RefreshUI()
    {
        GameData data = SaveSystem.LoadGame(slotID);

        if (data != null)
        {
            levelText.text = "Nível: " + data.playerLevel;
            loadButtonText.text = "Carregar";

            SetButtonInteractable(loadButton, true);
            SetButtonInteractable(eraseButton, true);
            SetButtonInteractable(copyButton, true);
        }
        else
        {
            levelText.text = "Nível: --";

            if (GameManager.dataToCopy != null)
                loadButtonText.text = "Colar";
            else
                loadButtonText.text = "Jogo Novo";

            SetButtonInteractable(loadButton, true);
            SetButtonInteractable(eraseButton, false);
            SetButtonInteractable(copyButton, false);
        }
    }

    private void SetButtonInteractable(Button btn, bool interactable)
    {
        if (btn == null) return;
        btn.interactable = interactable;
    }

    public void OnLoadClicked()
    {
        GameManager.Instance.SetCurrentSlot(slotID);

        if (SaveSystem.SaveFileExists(slotID))
        {
            // Trava os botões para evitar cliques duplicados durante a transição
            SetButtonInteractable(loadButton, false);
            SetButtonInteractable(eraseButton, false);
            SetButtonInteractable(copyButton, false);

            GameManager.Instance.LoadGame(slotID);

            string cenaParaCarregar = GameManager.Instance.sceneToLoad;
            if (string.IsNullOrEmpty(cenaParaCarregar)) cenaParaCarregar = "Vila_01";

            GameManager.Instance.LoadSceneWithFade(cenaParaCarregar);
        }
        else if (GameManager.dataToCopy != null)
        {
            // Operação de cópia — não muda de cena, apenas atualiza a UI
            SaveSystem.SaveGame(GameManager.dataToCopy, slotID);
            GameManager.dataToCopy = null;
            UpdateAllSlotUIs();
        }
        else
        {
            // Inicia novo jogo — trava os botões antes de mudar de cena
            SetButtonInteractable(loadButton, false);
            SetButtonInteractable(eraseButton, false);
            SetButtonInteractable(copyButton, false);

            GameManager.Instance.CreateNewGame("Herói");
        }
    }

    public void OnEraseClicked()
    {
        SaveSystem.EraseGame(slotID);

        if (GameManager.dataToCopy != null && SaveSystem.LoadGame(slotID) == null)
            GameManager.dataToCopy = null;

        UpdateAllSlotUIs();
    }

    public void OnCopyClicked()
    {
        GameData dataToCopy = SaveSystem.LoadGame(slotID);
        if (dataToCopy != null)
        {
            GameManager.dataToCopy = dataToCopy;
            UpdateAllSlotUIs();
        }
    }

    private void UpdateAllSlotUIs()
    {
        SaveSlotUI[] allSlots = FindObjectsByType<SaveSlotUI>(FindObjectsSortMode.None);
        foreach (SaveSlotUI slot in allSlots)
            slot.RefreshUI();
    }
}