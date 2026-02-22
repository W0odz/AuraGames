using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Referência")]
    public Image telaDeFade;

    [Header("Configuração")]
    public float duracaoFadeIn = 0.5f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (telaDeFade == null) yield break;

        telaDeFade.gameObject.SetActive(true);

        Color c = telaDeFade.color;
        c.a = 1f; // começa preto
        telaDeFade.color = c;

        float t = 0f;
        while (t < duracaoFadeIn)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duracaoFadeIn);
            telaDeFade.color = c;
            yield return null;
        }

        c.a = 0f;
        telaDeFade.color = c;
        telaDeFade.gameObject.SetActive(false);
    }
}