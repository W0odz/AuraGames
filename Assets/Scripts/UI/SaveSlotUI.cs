using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // Usado para encontrar os outros slots

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

        // Adiciona os "listeners" aos botões
        loadButton.onClick.AddListener(OnLoadClicked);
        eraseButton.onClick.AddListener(OnEraseClicked);
        copyButton.onClick.AddListener(OnCopyClicked);
    }

    // Esta função será chamada por outros slots quando
    // o estado de "copiar" mudar
    public void RefreshUI()
    {
        GameData data = SaveSystem.LoadGame(slotID);

        if (data != null)
        {
            // --- SE EXISTE UM SAVE ---
            levelText.text = "Nível: " + data.playerLevel;
            loadButtonText.text = "Carregar";

            loadButton.interactable = true;
            eraseButton.interactable = true;
            copyButton.interactable = true; // Pode copiar um save que existe
        }
        else
        {
            // --- SE O SLOT ESTÁ VAZIO ---
            levelText.text = "Nível: --";

            // Verifica se há algo na "área de transferência"
            if (GameManager.dataToCopy != null)
            {
                loadButtonText.text = "Colar"; // "Paste"
            }
            else
            {
                loadButtonText.text = "Jogo Novo";
            }

            loadButton.interactable = true;
            eraseButton.interactable = false;
            copyButton.interactable = false; // Não pode copiar um slot vazio
        }
    }

    // Chamado quando o botão "Load / Jogo Novo / Colar" é clicado
    public void OnLoadClicked()
    {
        // 1. Define o slot atual no GameManager
        GameManager.instance.SetCurrentSlot(slotID);

        // 2. Decide a ação
        if (SaveSystem.SaveFileExists(slotID))
        {
            // --- Carregar Jogo Existente ---
            GameManager.instance.LoadGame(slotID);

            // Carrega a cena de exploração
            GameManager.instance.LoadSceneWithFade("ExplorationScene");
        }
        else if (GameManager.dataToCopy != null)
        {
            // --- Colar Jogo (Este slot está vazio, mas o clipboard não) ---
            SaveSystem.SaveGame(GameManager.dataToCopy, slotID);

            // Limpa o clipboard
            GameManager.dataToCopy = null;

            // Atualiza todos os slots
            UpdateAllSlotUIs();
        }
        else
        {
            // --- Criar Jogo Novo ---
            GameManager.instance.CreateNewGame();

            // Carrega a cena de exploração
            GameManager.instance.LoadSceneWithFade("ExplorationScene");
        }
    }

    // Chamado quando o botão "Erase" é clicado
    public void OnEraseClicked()
    {
        SaveSystem.EraseGame(slotID);

        // Se estávamos copiando este slot, limpa o clipboard
        if (GameManager.dataToCopy != null && SaveSystem.LoadGame(slotID) == null)
        {
            GameManager.dataToCopy = null;
        }

        // Atualiza todos os slots
        UpdateAllSlotUIs();
    }

    // Chamado quando o botão "Copy" é clicado
    public void OnCopyClicked()
    {
        // Pega os dados deste slot e os coloca no clipboard
        GameData dataToCopy = SaveSystem.LoadGame(slotID);
        if (dataToCopy != null)
        {
            GameManager.dataToCopy = dataToCopy;
            Debug.Log("Slot " + slotID + " copiado!");

            // Atualiza todos os outros slots para mostrar a opção "Colar"
            UpdateAllSlotUIs();
        }
    }

    // Uma função "helper" que avisa todos os outros slots para se atualizarem
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