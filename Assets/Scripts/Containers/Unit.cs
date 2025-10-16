using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int damage;

    public int maxHP;
    public int currentHP;

    public int maxMP;
    public int currentMP;
    public bool isDefending = false;

    public SpriteRenderer spriteRenderer;

    // Função para receber dano
    public bool TakeDamage(int dmg)
    {
        if (isDefending)
        {
            dmg /= 2;
        }

        currentHP -= dmg;

        if (currentHP <= 0)
        {
            currentHP = 0;
            return true; // Morreu
        }
        else
        {
            return false; // Continua vivo
        }
    }
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public IEnumerator FadeOut()
    {
        float fadeDuration = 1f; // Duração do fade em segundos
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            // Incrementa o tempo passado
            elapsedTime += Time.deltaTime;

            // Calcula o novo valor de alpha (opacidade)
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            // Aplica a nova cor com o alpha modificado
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            // Espera até o próximo frame
            yield return null;
        }

        // Garante que a opacidade seja 0 no final
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

}