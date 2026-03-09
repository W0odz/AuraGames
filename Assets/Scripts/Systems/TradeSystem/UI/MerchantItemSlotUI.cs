using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class MerchantItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icone;
    public Button botao;
    public TextMeshProUGUI textoQtd; // ← opcional, pode ser null

    private DadosItem _item;

    public void Setup(DadosItem item, Action onClick, int quantidade = 0)
    {
        _item = item;
        icone.sprite = item.iconeItem;

        // Mostra quantidade se o campo existir e for > 0
        if (textoQtd != null)
            textoQtd.text = quantidade > 0 ? $"x{quantidade}" : "";

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => onClick());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item != null)
            TooltipManager.Instance.Show(_item.nomeItem, _item.descricao, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.Hide();
    }
}