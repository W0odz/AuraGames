// PauseManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Painéis")]
    public GameObject pausePanel;
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
    public string parametroGeral   = "VolumeGeral";
    public string parametroSFX     = "VolumeSFX";
    public string parametroMusica  = "VolumeMusica";

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (configPanel   != null) configPanel.SetActive(false);
        if (soundPanel    != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pausePanel    != null) pausePanel.SetActive(false);

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
        if (GameManager.Instance != null && GameManager.Instance.inputBloqueado) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ── Pausa / Continuar ──────────────────────────────────────────

    public void PauseGame()
    {
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.isOpen) return;

        isPaused = true;
        MostrarPainelPrincipal();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        FecharTodosPaineis();
        Time.timeScale = 1f;
    }

    private void MostrarPainelPrincipal()
    {
        if (pausePanel    != null) pausePanel.SetActive(true);
        if (configPanel   != null) configPanel.SetActive(false);
        if (soundPanel    != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    private void FecharTodosPaineis()
    {
        if (pausePanel    != null) pausePanel.SetActive(false);
        if (configPanel   != null) configPanel.SetActive(false);
        if (soundPanel    != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // ── Navegação ──────────────────────────────────────────────────

    public void OnContinuarButton()          => ResumeGame();

    public void OnConfiguracoesButton()
    {
        if (pausePanel  != null) pausePanel.SetActive(false);
        if (configPanel != null) configPanel.SetActive(true);
    }

    public void OnSairSemSalvarButton()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithFade("TitleScreen");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
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

    public void OnVoltarDeConfiguracoesButton()
    {
        if (configPanel != null) configPanel.SetActive(false);
        if (pausePanel  != null) pausePanel.SetActive(true);
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

    // ── Áudio ──────────────────────────────────────────────────────

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

    // Compatibilidade com referência antiga
    public void OnMenuButton() => OnSairSemSalvarButton();
}