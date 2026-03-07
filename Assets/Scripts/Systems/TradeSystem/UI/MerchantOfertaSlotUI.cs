using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantOfertaSlotUI : MonoBehaviour
{
    public Image icone;
    public TextMeshProUGUI textoQtd;
    public Button botaoRemover;

    private DadosItem _item;
    private MerchantMenuUI _menu;

    public void Setup(DadosItem item, int qty, MerchantMenuUI menu)
    {
        _item = item;
        _menu = menu;

        icone.sprite = item.iconeItem;
        textoQtd.text = $"{item.nomeItem}  x{qty}";

        botaoRemover.onClick.RemoveAllListeners();
        botaoRemover.onClick.AddListener(() => _menu.RemoverItemOferta(_item));
    }
}