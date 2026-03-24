    private IEnumerator AutoOcultarCoroutine(bool imediato)
    {
        if (!imediato)
            yield return new WaitForSecondsRealtime(tempoAutoOcultar);

        // Fade out nome e objetivo ao mesmo tempo
        float elapsed = 0f;
        float alphaInicio = 1f;

        while (elapsed < duracaoFade)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(alphaInicio, 0f, elapsed / duracaoFade);

            if (textoNomeQuest != null)
            {
                Color c = textoNomeQuest.color;
                c.a = alpha;
                textoNomeQuest.color = c;
            }
            if (textoObjetivo != null)
            {
                Color c = textoObjetivo.color;
                c.a = alpha;
                textoObjetivo.color = c;
            }
            yield return null;
        }

        // Garantir alpha 0
        if (textoNomeQuest != null) { Color c = textoNomeQuest.color; c.a = 0f; textoNomeQuest.color = c; }
        if (textoObjetivo != null)  { Color c = textoObjetivo.color;  c.a = 0f; textoObjetivo.color = c; }

        if (painel != null)
            painel.SetActive(false);
    }