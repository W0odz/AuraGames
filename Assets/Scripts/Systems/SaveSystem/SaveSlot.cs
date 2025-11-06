// SaveSlot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlot : MonoBehaviour
{
    [Header("Configuração do Slot")]
    public int slotID; // Defina 1, 2 ou 3 no Inspector

    [Header("Referências da UI")]
    public TextMeshProUGUI mainText; // O texto "Jogo novo" ou "Herói"
    public TextMeshProUGUI levelText; // O texto "Nível 5" ou "Vazio"

    private Button thisButton;

    void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnSlotClicked);

        CheckForSaveFile();
    }

    // Verifica se um save existe e atualiza a UI
    public void CheckForSaveFile()
    {
        GameData data = SaveSystem.LoadGame(slotID);

        if (data != null)
        {
            // TEM SAVE: Mostra os dados do save
            mainText.text = data.playerName;
            levelText.text = "Nível " + data.playerLevel;
        }
        else
        {
            // NÃO TEM SAVE: Mostra "Jogo novo"
            mainText.text = "Jogo novo";
            levelText.text = "Vazio";
        }
    }

    // Chamado quando o botão é clicado
    public void OnSlotClicked()
    {
        // 1. Define o slot atual no GameManager
        GameManager.instance.SetCurrentSlot(slotID);

        // 2. Decide se carrega um jogo ou cria um novo
        if (SaveSystem.SaveFileExists(slotID))
        {
            GameManager.instance.LoadGame(slotID);
        }
        else
        {
            GameManager.instance.CreateNewGame();
        }

        // 3. Carrega a cena de exploração
        GameManager.instance.LoadSceneWithFade("ExplorationScene");
    }
}