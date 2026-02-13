using UnityEngine;

public class Campfire : MonoBehaviour
{
    public GameObject painelCrafting; // Arraste seu painel de UI aqui
    private bool jogadorPerto = false;

    void Update()
    {
        // Se o jogador estiver perto e apertar uma tecla (ex: E ou Espaço)
        if (jogadorPerto && Input.GetKeyDown(KeyCode.E))
        {
            AbrirMenuCrafting();
        }
    }

    void AbrirMenuCrafting()
    {
        painelCrafting.SetActive(true);
        // Opcional: Pausar o jogo ou congelar o movimento do player
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) jogadorPerto = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            painelCrafting.SetActive(false); // Fecha ao se afastar
        }
    }
}