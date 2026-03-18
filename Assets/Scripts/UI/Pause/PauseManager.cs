// PauseManager.cs — controla tanto o menu de pause do jogo quanto a tela inicial
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Modo")]
    [Tooltip("Marque TRUE na tela inicial, FALSE nas cenas de jogo")]
    public bool modoTituloAtivo = false;

    [Header("Painéis — Tela Inicial")]
    [Tooltip("O painel com os 3 slots de save (só usado na tela inicial)")]
    public GameObject saveSlotsPanel;

    [Header("Painéis — Pause (jogo)")]
    public GameObject pausePanel;

    [Header("Painéis — Compartilhados")]
    public GameObject configPanel;
    public GameObject soundPanel;
    public GameObject controlsPanel;

    [Header("Sliders de Áudio")]
    public Slider sliderGeral;
    public Slider sliderSFX;
    public Slider sliderMusica;

    [Header("Audio Mixer (opcional)")]
    public AudioMixer audioMixer;

    [Header("Nomes dos Parâmetros no AudioMixer")]
    public string parametroGeral  = "VolumeGeral";
    public string parametroSFX    = "VolumeSFX";
    public string parametroMusica = "VolumeMusica";

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Fecha todos os painéis ao iniciar
        FecharTodosPaineis();

        // Na tela inicial, mostra os slots de save como tela principal
        if (modoTituloAtivo && saveSlotsPanel != null)
            saveSlotsPanel.SetActive(false); // começa fechado até clicar Play

        // Inicializa sliders de áudio com valores salvos
        float vGeral  = PlayerPrefs.GetFloat("VolumeGeral",  1f);
        float vSFX    = PlayerPrefs.GetFloat("VolumeSFX",    1f);
        float vMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        if (sliderGeral  != null) sliderGeral.value  = vGeral;
        if (sliderSFX    != null) sliderSFX.value    = vSFX;
        if (sliderMusica != null) sliderMusica.value = vMusica;

        AplicarVolumeGeral(vGeral);
        AplicarVolumeSFX(vSFX);
        AplicarVolumeMusica(vMusica);

        if (sliderGeral  != null) sliderGeral.onValueChanged.AddListener(AplicarVolumeGeral);
        if (sliderSFX    != null) sliderSFX.onValueChanged.AddListener(AplicarVolumeSFX);
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(AplicarVolumeMusica);
    }

    private void Update()
    {
        // ESC só funciona no modo jogo (pause), não na tela inicial
        if (modoTituloAtivo) return;
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ── Tela Inicial ───────────────────────────────────────────────

    /// <summary>Chamado pelo botão "Play" na tela inicial.</summary>
    public void OnPlayButton()
    {
        FecharTodosPaineis();
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
    }

    /// <summary>Chamado pelo botão "Voltar" dentro do painel de saves.</summary>
    public void OnReturnButton()
    {
        FecharTodosPaineis();
    }

    // ── Pause / Continuar (jogo) ───────────────────────────────────

    public void PauseGame()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.isOpen) return;

        isPaused = true;
        MostrarPainelPrincipalJogo();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        FecharTodosPaineis();
        Time.timeScale = 1f;
    }

    public void OnContinuarButton() => ResumeGame();

    // ── Configurações (compartilhado: funciona na tela inicial e no pause) ──

    public void OnConfiguracoesButton()
    {
        // Fecha o painel que estiver aberto (saves ou pause)
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (pausePanel     != null) pausePanel.SetActive(false);
        if (configPanel    != null) configPanel.SetActive(true);
    }

    public void OnVoltarDeConfiguracoesButton()
    {
        if (configPanel != null) configPanel.SetActive(false);

        // No modo título: só fecha tudo, não reabre o painel de saves
        if (!modoTituloAtivo)
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }
    }

    public void OnSomMusicaButton()
    {
        if (configPanel != null) configPanel.SetActive(false);
        if (soundPanel  != null) soundPanel.SetActive(true);
    }

    public void OnControlesButton()
    {
        if (configPanel   != null) configPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void OnVoltarDeSomButton()
    {
        if (soundPanel  != null) soundPanel.SetActive(false);
        if (configPanel != null) configPanel.SetActive(true);
    }

    public void OnVoltarDeControlesButton()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (configPanel   != null) configPanel.SetActive(true);
    }

    // ── Sair ──────────────────────────────────────────────────────

    public void OnSairSemSalvarButton()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsShuttingDown) return;

        Time.timeScale = 1f;
        isPaused = false;
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithFade("TitleScreen");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
    }

    public void OnSairDoJogoButton()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }

    // Compatibilidade com referência antiga
    public void OnMenuButton() => OnSairSemSalvarButton();

    // ── Helpers ───────────────────────────────────────────────────

    private void MostrarPainelPrincipalJogo()
    {
        if (pausePanel    != null) pausePanel.SetActive(true);
        if (configPanel   != null) configPanel.SetActive(false);
        if (soundPanel    != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    private void FecharTodosPaineis()
    {
        if (pausePanel     != null) pausePanel.SetActive(false);
        if (configPanel    != null) configPanel.SetActive(false);
        if (soundPanel     != null) soundPanel.SetActive(false);
        if (controlsPanel  != null) controlsPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    // ── Áudio ─────────────────────────────────────────────────────

    public void AplicarVolumeGeral(float valor)
    {
        PlayerPrefs.SetFloat("VolumeGeral", valor);
        if (audioMixer != null)
        {
            float db = valor > 0.0001f ? Mathf.Log10(valor) * 20f : -80f;
            audioMixer.SetFloat(parametroGeral, db);
        }
        else
        {
            AudioListener.volume = valor;
        }
    }

    public void AplicarVolumeSFX(float valor)
    {
        PlayerPrefs.SetFloat("VolumeSFX", valor);
        if (audioMixer != null)
        {
            float db = valor > 0.0001f ? Mathf.Log10(valor) * 20f : -80f;
            audioMixer.SetFloat(parametroSFX, db);
        }
    }

    public void AplicarVolumeMusica(float valor)
    {
        PlayerPrefs.SetFloat("VolumeMusica", valor);
        if (audioMixer != null)
        {
            float db = valor > 0.0001f ? Mathf.Log10(valor) * 20f : -80f;
            audioMixer.SetFloat(parametroMusica, db);
        }
    }
}