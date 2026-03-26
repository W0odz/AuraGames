using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleHUD : MonoBehaviour
{
    public static BattleHUD Instance;

    [Header("Configura��es de Tipo")]
    public bool ehInimigo; // Marque True no Inspector do HUD do inimigo
    public float tempoVisivel = 2.5f; // Quanto tempo a barra fica na tela
    private float cronometroVisibilidade;

    [Header("Componentes")]
    public CanvasGroup canvasGroup; // Para fazer o Fade In/Out
    public Image portraitImage;
    public GameObject commandsPanel;

    [Header("Botões de Ação do Jogador")]
    [Tooltip("Arraste aqui o botão de Atacar do commandsPanel.")]
    public Button botaoAtacar;
    [Tooltip("Arraste aqui o botão de Item do commandsPanel.")]
    public Button botaoItem;
    [Tooltip("Arraste aqui o botão de Fugir do commandsPanel.")]
    public Button botaoFugir;

    [Header("HP - Suave")]
    public TextMeshProUGUI hpText;
    public Slider hpSlider;

    // NOVO: arraste aqui o Image do "Fill" do slider de HP (Fill Area/Fill)
    public Image hpFillImage;

    private float valorAlvoHP;

    [Header("MP - Suave")]
    public TextMeshProUGUI mpText;
    public Slider mpSlider;
    private float valorAlvoMP;

    [Header("Configura��es Visuais")]
    public float velocidadeSuavizacao = 5f;

    private bool bloquearAutoHide = false;

    // NOVO: um epsilon pra evitar problemas de float (e evitar piscar perto do 0)
    private const float HP_EPSILON = 0.001f;

    private void Awake()
    {
        if (Instance == null && !ehInimigo) Instance = this;

        // Se for inimigo, come�a invis�vel
        if (ehInimigo && canvasGroup != null) canvasGroup.alpha = 0;
    }

    void Update()
    {
        if (hpSlider != null && hpSlider.value != valorAlvoHP)
        {
            hpSlider.value = Mathf.MoveTowards(
                hpSlider.value,
                valorAlvoHP,
                velocidadeSuavizacao * Time.deltaTime * hpSlider.maxValue
            );
            AtualizarTextoHP((int)hpSlider.value, (int)hpSlider.maxValue);
        }

        // NOVO: some com o preenchimento quando HP chegar em ~0
        AtualizarVisibilidadeFillHP();

        if (ehInimigo && !bloquearAutoHide && cronometroVisibilidade > 0)
        {
            cronometroVisibilidade -= Time.deltaTime;
            if (cronometroVisibilidade <= 0) StartCoroutine(FadeHUD(0f)); // Desaparece no fim
        }
    }

    private void AtualizarVisibilidadeFillHP()
    {
        if (hpFillImage == null || hpSlider == null) return;

        // enabled=false evita aquele "restinho" visual do Sliced quando value � 0
        hpFillImage.enabled = hpSlider.value > HP_EPSILON;
    }

    public void SetHUD(Unit unit)
    {
        gameObject.SetActive(true);

        if (portraitImage != null && unit.unitPortrait != null)
            portraitImage.sprite = unit.unitPortrait;

        if (hpSlider != null)
        {
            hpSlider.maxValue = unit.maxHP;
            hpSlider.value = unit.currentHP;
            valorAlvoHP = unit.currentHP; // Inicializa o alvo
            AtualizarTextoHP(unit.currentHP, unit.maxHP);
        }

        // NOVO: garante o estado correto j� na inicializa��o
        AtualizarVisibilidadeFillHP();

        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMP;
            mpSlider.value = unit.currentMP;
            valorAlvoMP = unit.currentMP;
            AtualizarTextoMP(unit.currentMP, unit.maxMP);
        }

        if (ehInimigo && canvasGroup != null) canvasGroup.alpha = 0;
    }

    public void UpdateHP(int currentHp)
    {
        valorAlvoHP = currentHp;

        if (ehInimigo && !bloquearAutoHide)
        {
            cronometroVisibilidade = tempoVisivel;

            StopAllCoroutines();
            StartCoroutine(FadeHUD(1f)); // Aparece ao tomar dano
        }
    }

    public void UpdateMP(int currentMp)
    {
        valorAlvoMP = currentMp;
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

    public IEnumerator FadeOutAndWait()
    {
        bloquearAutoHide = true;
        cronometroVisibilidade = 0f;

        StopAllCoroutines();

        if (canvasGroup == null) yield break;
        if (Mathf.Approximately(canvasGroup.alpha, 0f)) yield break;

        yield return StartCoroutine(FadeHUD(0f));
    }

    /// <summary>Desabilita os botões de ação do jogador (fora do turno do jogador).</summary>
    public void BloquearBotoes()
    {
        SetBotoesInteractable(false);
    }

    /// <summary>Habilita os botões de ação do jogador (início do turno do jogador).</summary>
    public void DesbloquearBotoes()
    {
        SetBotoesInteractable(true);
    }

    private void SetBotoesInteractable(bool valor)
    {
        if (botaoAtacar != null)
        {
            botaoAtacar.interactable = valor;
            botaoAtacar.transition = valor ? Selectable.Transition.ColorTint : Selectable.Transition.None;
        }
        if (botaoItem != null)
        {
            botaoItem.interactable = valor;
            botaoItem.transition = valor ? Selectable.Transition.ColorTint : Selectable.Transition.None;
        }
        if (botaoFugir != null)
        {
            botaoFugir.interactable = valor;
            botaoFugir.transition = valor ? Selectable.Transition.ColorTint : Selectable.Transition.None;
        }
    }

    public void MostrarMenuPrincipal()
    {
        if (commandsPanel != null) commandsPanel.SetActive(true);
        DesbloquearBotoes();
    }
}