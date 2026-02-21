using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleHUD : MonoBehaviour
{
    public static BattleHUD Instance;

    [Header("Configurações de Tipo")]
    public bool ehInimigo; // Marque True no Inspector do HUD do inimigo
    public float tempoVisivel = 2.5f; // Quanto tempo a barra fica na tela
    private float cronometroVisibilidade;

    [Header("Componentes")]
    public CanvasGroup canvasGroup; // Para fazer o Fade In/Out
    public Image portraitImage;
    public GameObject commandsPanel;

    [Header("HP - Suave")]
    public TextMeshProUGUI hpText;
    public Slider hpSlider;
    private float valorAlvoHP;

    [Header("MP - Suave")]
    public TextMeshProUGUI mpText; 
    public Slider mpSlider;       
    private float valorAlvoMP;

    [Header("Configurações Visuais")]
    public float velocidadeSuavizacao = 5f;

    private void Awake()
    {
        if (Instance == null && !ehInimigo) Instance = this;

        // Se for inimigo, começa invisível
        if (ehInimigo && canvasGroup != null) canvasGroup.alpha = 0;
    }

    public void SetHUD(Unit unit)
    {
        if (portraitImage != null && unit.unitPortrait != null)
            portraitImage.sprite = unit.unitPortrait;

        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
        valorAlvoHP = unit.currentHP; // Inicializa o alvo
        AtualizarTextoHP(unit.currentHP, unit.maxHP);

        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMP;
            mpSlider.value = unit.currentMP;
            valorAlvoMP = unit.currentMP;
            AtualizarTextoMP(unit.currentMP, unit.maxMP);
        }
    }

    // Atualiza o alvo da vida, não o valor direto
    public void UpdateHP(int currentHp)
    {
        valorAlvoHP = currentHp;

        // Se for inimigo, "acorda" o HUD ao tomar dano
        if (ehInimigo)
        {
            cronometroVisibilidade = tempoVisivel;
            StopAllCoroutines();
            StartCoroutine(FadeHUD(1f)); // Aparece
        }
    }

    public void UpdateMP(int currentMp)
    {
        valorAlvoMP = currentMp;
    }

    void Update()
    {
        // Movimento suave da barra (Lerp/MoveTowards)
        if (hpSlider.value != valorAlvoHP)
        {
            hpSlider.value = Mathf.MoveTowards(hpSlider.value, valorAlvoHP, velocidadeSuavizacao * Time.deltaTime * hpSlider.maxValue);
            AtualizarTextoHP((int)hpSlider.value, (int)hpSlider.maxValue);
        }

        if (mpSlider != null && mpSlider.value != valorAlvoMP)
        {
            mpSlider.value = Mathf.MoveTowards(mpSlider.value, valorAlvoMP, velocidadeSuavizacao * Time.deltaTime * mpSlider.maxValue);
            AtualizarTextoMP((int)mpSlider.value, (int)mpSlider.maxValue);
        }

        if (ehInimigo && cronometroVisibilidade > 0)
        {
            cronometroVisibilidade -= Time.deltaTime;
            if (cronometroVisibilidade <= 0) StartCoroutine(FadeHUD(0f));
        }
    }

    void AtualizarTextoHP(int atual, int max)
    {
        if (hpText != null)
            hpText.text = $"{atual:00} / {max:00}";
    }

    void AtualizarTextoMP(int atual, int max)
    {
        if (mpText != null) mpText.text = $"{atual:00} / {max:00}";
    }

    IEnumerator FadeHUD(float targetAlpha)
    {
        if (canvasGroup == null) yield break;
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * 2f);
            yield return null;
        }
    }

    public void MostrarMenuPrincipal()
    {
        if (commandsPanel != null) commandsPanel.SetActive(true);
    }
}