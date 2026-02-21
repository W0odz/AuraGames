using UnityEngine;
using TMPro;

public class NameInputPopup : MonoBehaviour
{
    public TMP_InputField nameInputField; // Arraste o InputField aqui
    private int slotToCreate; // Guarda qual slot foi clicado
    public GameObject saveSlotsPanel;

    // Chamado pelo SaveSlotUI para abrir a janela
    public void OpenPopup(int slotID)
    {
        slotToCreate = slotID;
        nameInputField.text = ""; // Limpa o texto anterior
        gameObject.SetActive(true); // Mostra a janela

        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }
    }

    // Chamado pelo botão "OK"
    public void OnConfirmButton()
    {
        string chosenName = nameInputField.text;

        // 1. Define o slot
        GameManager.Instance.SetCurrentSlot(slotToCreate);

        // 2. Cria o jogo com o nome escolhido
        GameManager.Instance.CreateNewGame(chosenName);

        // 3. Carrega a cena
        GameManager.Instance.LoadSceneWithFade("ExplorationScene");

        // Fecha o popup (opcional, já que vamos mudar de cena)
        gameObject.SetActive(false);
    }
    public void OnCancelButton()
    {
        gameObject.SetActive(false); // Esconde este pop-up

        // --- RESTAURA O PAINEL DE TRÁS ---
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(true);
        }
    }
}