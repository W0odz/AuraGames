using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleItemSlotUI : MonoBehaviour
{
    [Header("Referências")]
    public Image icone;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoEfeito;
    public TextMeshProUGUI textoQuantidade;
    public Button botaoUsar;

    private DadosItem _item;

    public void Setup(DadosItem item, int quantidade)
    {
        _item = item;

        icone.sprite = item.iconeItem;
        textoNome.text = item.nomeItem;
        textoEfeito.text = item.descricao;
        textoQuantidade.text = "x" + quantidade;

        botaoUsar.onClick.RemoveAllListeners();
        botaoUsar.onClick.AddListener(UsarItem);
    }

    private void UsarItem()
    {
        if (_item == null) return;

        // 1. Executa o efeito (Heal já altera o currentHP corretamente)
        _item.Use(PlayerUnit.Instance.gameObject);

        // 2. Remove do inventário
        InventoryManager.Instance.RemoverItem(_item, 1);

        // 3. Atualiza a HUD para refletir o novo HP
        BattleSystem.Instance.playerHUD.UpdateHP(PlayerUnit.Instance.currentHP);

        // 4. Fecha o painel
        BattleItemPanelUI.Instance.Fechar();

        // 5. Passa o turno
        BattleSystem.Instance.PassarTurnoAposItem();
    }
}