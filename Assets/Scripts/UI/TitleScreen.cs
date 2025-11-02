using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para trocar de cena

public class MenuPrincipal : MonoBehaviour
{
    // O nome exato do seu arquivo de cena de exploração
    public string explorationSceneName = "ExplorationScene";

    // Esta função será chamada pelo nosso botão
    public void OnPlayButton()
    {
        // Carrega a cena de exploração
        GameManager.instance.LoadSceneWithFade(explorationSceneName);
    }
}