using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{

    public static BattleHUD Instance;

    [Header("Painéis de Controle")]
    public GameObject commandsPanel;

    [Header("Identificação")]
    public TextMeshProUGUI nameText;

    [Header("HP - Coração")]
    public TextMeshProUGUI hpText;
    public Slider hpSlider;

    [Header("MP - Energia")]
    public TextMeshProUGUI mpText;
    public Slider mpSlider;


    private void Awake()
    {
        if (Instance == null) Instance = this; // E isto
    }

    public void MostrarMenuPrincipal()
    {
        if (commandsPanel != null)
        {
            // Reativa os botões de Atacar, Itens, etc.
            commandsPanel.SetActive(true);
        }
    }

    // Configura o HUD no início da batalha
    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;

        // Configuração de Vida
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
        hpText.text = $"{unit.currentHP:00}/{unit.maxHP:00}"; // Formato 07/12

        // Configuração de MP
        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMP;
            mpSlider.value = unit.currentMP;
            mpText.text = $"{unit.currentMP:00}/{unit.maxMP:00}";
        }
    }

    public void UpdateHP(int currentHp)
    {
        hpSlider.value = currentHp;
        hpText.text = $"{currentHp:00}/{hpSlider.maxValue:00}";
    }

    public void UpdateMP(int currentMp)
    {
        if (mpSlider != null)
        {
            mpSlider.value = currentMp;
            mpText.text = $"{currentMp:00}/{mpSlider.maxValue:00}";
        }
    }
}