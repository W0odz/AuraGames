using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionLogEntry : MonoBehaviour
{
    public TextMeshProUGUI logText;
    public Image itemIcon;
    public CanvasGroup canvasGroup; // Arraste o componente daqui para o Inspector
    public float tempoDeVida = 3f;

    public void Setup(DadosItem item, int quantidade)
    {
        logText.text = $"+ {quantidade}";
        if (item.iconeItem != null) itemIcon.sprite = item.iconeItem;

        // Garante que ele nasce com opacidade total
        canvasGroup.alpha = 1f;

        Destroy(gameObject, tempoDeVida);
    }

    // Função para reduzir a opacidade gradualmente ou direto
    public void DiminuirOpacidade(float novoAlpha)
    {
        canvasGroup.alpha = novoAlpha;
    }
}