using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class EquipmentPanel : MonoBehaviour
{
    [Header("Sprites de Interface")]
    public Sprite spriteVazio; // Arraste aqui o seu quadrado branco ou ícone de fundo

    [Header("Textos das Rows (Arraste aqui)")]
    public TextMeshProUGUI txtWeapon;
    public TextMeshProUGUI txtHelmet;
    public TextMeshProUGUI txtChestplate;
    public TextMeshProUGUI txtGloves;
    public TextMeshProUGUI txtLegs;

    [Header("Imagens dos Ícones (Arraste aqui)")]
    public Image imgWeapon;
    public Image imgHelmet;
    public Image imgChestplate;
    public Image imgGloves;
    public Image imgLegs;

    [Header("Referência da Mochila")]
    public InventoryUIManager uiMochila;

    // Chamado sempre que o objeto do inventário é ativado
    void OnEnable()
    {
        if (EquipmentManager.Instance != null)
        {
            // Se inscreve no evento para atualizar caso você troque de arma com o menu aberto
            EquipmentManager.Instance.onEquipmentChanged -= AtualizarTudo;
            EquipmentManager.Instance.onEquipmentChanged += AtualizarTudo;
            AtualizarTudo();
        }
    }

    void OnDisable()
    {
        // Importante remover a inscrição ao fechar o menu para evitar erros de memória
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= AtualizarTudo;
    }

    public void AtualizarTudo()
    {
        // 1. Atualiza os 5 slots centrais (sua lógica original)
        AtualizarPainel();

        // 2. FORÇA a mochila lateral a se atualizar na hora
        if (uiMochila != null)
        {
            uiMochila.AtualizarVisual();
        }
    }

    public void AtualizarPainel()
    {
        var eq = EquipmentManager.Instance.equipamentosAtuais;

        // Atualizamos cada slot (Texto e Imagem)
        ConfigurarSlotUI(SlotEquipamento.Weapon, txtWeapon, imgWeapon);
        ConfigurarSlotUI(SlotEquipamento.Helmet, txtHelmet, imgHelmet);
        ConfigurarSlotUI(SlotEquipamento.Chestplate, txtChestplate, imgChestplate);
        ConfigurarSlotUI(SlotEquipamento.Gloves, txtGloves, imgGloves);
        ConfigurarSlotUI(SlotEquipamento.Legs, txtLegs, imgLegs);
    }

    private void ConfigurarSlotUI(SlotEquipamento slot, TextMeshProUGUI texto, Image imagem)
    {
        DadosItem item = EquipmentManager.Instance.equipamentosAtuais[(int)slot];

        // O componente Image agora fica SEMPRE ativado
        imagem.enabled = true;

        if (item != null)
        {
            texto.text = item.nomeItem;
            imagem.sprite = item.iconeItem;
            imagem.color = Color.white; // Opacidade total para o item real
        }
        else
        {
            texto.text = "---";
            imagem.sprite = spriteVazio; // Coloca o quadrado branco/transparente

            // DICA: Você pode deixar o quadrado branco um pouco transparente 
            // para dar aquele efeito de "espaço vazio"
            imagem.color = new Color(1, 1, 1, 0.2f);
        }
    }
}