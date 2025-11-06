using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public Slider hpSlider;

    public TextMeshProUGUI mpText;
    public Slider mpSlider;

    // Esta função configura o HUD no início da batalha com os dados da unidade
    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
        hpText.text = unit.currentHP + " / " + unit.maxHP;

        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMP;
            mpSlider.value = unit.currentMP;
            mpText.text = unit.currentMP + " / " + unit.maxMP;
        }
    }

    // Esta função atualiza apenas a vida (texto e slider)
    public void UpdateHP(int currentHp)
    {
        hpSlider.value = currentHp;
        hpText.text = currentHp + " / " + hpSlider.maxValue;
    }
    public void UpdateMP(int currentMp)
    {
        if (mpSlider != null)
        {
            mpSlider.value = currentMp;
            mpText.text = currentMp + " / " + mpSlider.maxValue;
        }
    }
}