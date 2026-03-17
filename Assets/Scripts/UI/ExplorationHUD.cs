using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD da cena de exploração — atualiza HP, XP, nível e Força de Vontade.
/// </summary>
public class ExplorationHUD : MonoBehaviour
{
    public static ExplorationHUD Instance;

    [Header("HP")]
    public Slider sliderHP;
    public TextMeshProUGUI textoHP;    // exibe "45 / 100"

    [Header("XP")]
    public Slider sliderXP;
    public TextMeshProUGUI textoNivel; // exibe "Nv. 3"

    [Header("Força de Vontade")]
    public Image iconeForcaDeVontade;
    public Color corAtiva   = Color.white;
    public Color corInativa = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        Atualizar();
    }

    public void Atualizar()
    {
        var player = PlayerUnit.Instance;
        if (player == null) return;

        // ── HP ───────────────────────────────────────────────────────
        if (sliderHP != null)
        {
            sliderHP.maxValue = player.maxHP;
            sliderHP.value    = player.currentHP;
        }

        if (textoHP != null)
            textoHP.text = $"{player.currentHP} / {player.maxHP}";

        // ── XP / Nível ───────────────────────────────────────────────
        if (sliderXP != null)
        {
            sliderXP.maxValue = player.xpToNextLevel;
            sliderXP.value    = player.currentXP;
        }

        if (textoNivel != null)
            textoNivel.text = $"Nv. {player.playerLevel}";

        // ── Força de Vontade ─────────────────────────────────────────
        if (iconeForcaDeVontade != null)
            iconeForcaDeVontade.color = player.temForcaDeVontade ? corAtiva : corInativa;
    }
}
