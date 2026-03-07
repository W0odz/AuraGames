using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantEstoqueSlotUI : MonoBehaviour
{
    public Image icone;
    public TextMeshProUGUI textoNome;
    public Button botao;

    private DadosItem _item;
    private MerchantMenuUI _menu;

    public void Setup(DadosItem item, MerchantMenuUI menu)
    {
        _item = item;
        _menu = menu;

        icone.sprite = item.iconeItem;
        textoNome.text = item.nomeItem;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => _menu.SelecionarItemDesejado(_item, 1));
    }
}