using System.Collections;
using UnityEngine;

/// <summary>
/// Gerencia a sequência de introdução do jogo.
/// Coloque em um GameObject vazio na cena inicial (Vila_01).
/// Detecta a flag triggerIntroOnLoad do GameManager e dispara:
///   1. Diálogo de pensamento do personagem
///   2. Início da primeira quest ao terminar o diálogo
/// </summary>
public class IntroSequence : MonoBehaviour
{
    [Header("Diálogo de Intro")]
    [Tooltip("DialogueAsset com o pensamento inicial do personagem.")]
    public DialogueAsset dialogoIntro;

    [Header("Primeira Quest")]
    [Tooltip("QuestDefinition da primeira quest do jogo.")]
    public QuestDefinition primeiraQuest;

    [Header("Delay após a cena carregar")]
    [Tooltip("Tempo em segundos antes de iniciar o diálogo (espera o fade in terminar).")]
    public float delayInicio = 0.8f;

    private IEnumerator Start()
    {
        // Só roda se vier de um novo jogo
        if (GameManager.Instance == null || !GameManager.Instance.triggerIntroOnLoad)
            yield break;

        // Consome a flag imediatamente para não rodar de novo
        GameManager.Instance.triggerIntroOnLoad = false;

        // Bloqueia input enquanto espera o fade in da cena terminar
        GameManager.Instance.inputBloqueado = true;

        yield return new WaitForSecondsRealtime(delayInicio);

        GameManager.Instance.inputBloqueado = false;

        if (dialogoIntro == null)
        {
            Debug.LogWarning("[IntroSequence] dialogoIntro não atribuído — pulando diálogo.");
            IniciarPrimeiraQuest();
            yield break;
        }

        // Inicia o diálogo; ao terminar, dispara a quest
        DialogueRunner.Instance.StartDialogue(dialogoIntro, () =>
        {
            IniciarPrimeiraQuest();
        });
    }

    private void IniciarPrimeiraQuest()
    {
        if (primeiraQuest == null)
        {
            Debug.LogWarning("[IntroSequence] primeiraQuest não atribuída.");
            return;
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.StartQuest(primeiraQuest);
        else
            Debug.LogWarning("[IntroSequence] QuestManager.Instance é null.");
    }
}