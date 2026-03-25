using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TitleScreenMusic : MonoBehaviour
{
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        // Desliga o PlayOnAwake — vamos controlar manualmente no Start
        _source.playOnAwake = false;
    }

    private IEnumerator Start()
    {
        // Espera 1 frame para garantir que o PauseManager.Start() rodou
        // e aplicou o volume correto no AudioMixer antes de Play()
        yield return null;

        if (!_source.isPlaying)
            _source.Play();
    }
}
