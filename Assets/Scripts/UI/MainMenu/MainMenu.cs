using UnityEngine;

/// <summary>
/// Stub de compatibilidade — lógica migrada para PauseManager.
/// Mantido para não quebrar referências existentes no Inspector.
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPlayButton();
    }

    public void OnReturnButtonClicked()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnReturnButton();
    }
}