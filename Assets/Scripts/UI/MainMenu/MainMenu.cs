using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    // Arraste o botão "Play" principal aqui
    public GameObject mainPlayButton;

    // Arraste o painel que contém os 3 slots aqui
    public GameObject saveSlotsPanel;

    // Esta função será chamada pelo 'PlayButton'
    public void OnPlayButtonClicked()
    {
        // Esconde o botão "Play"
        mainPlayButton.SetActive(false);

        // Mostra o painel com os slots de save
        saveSlotsPanel.SetActive(true);
    }

    public void OnReturnButtonClicked()
    {
        // Mostra o botão "Play"
        mainPlayButton.SetActive(true);

        // Esconde o painel com os slots de save
        saveSlotsPanel.SetActive(false);

    }

}