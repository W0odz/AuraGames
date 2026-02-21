using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    [Header("Identidade Base")]
    public string unitName;
    public Sprite unitPortrait;
    public int playerLevel;
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;

    [Header("Atributos Base")]
    public int strength;
    public int resistance;
    public int speed;
    public int luck;

    [Header("Estado de Combate")]
    public bool isDefending = false; 
    public int xpValue;             

    public SpriteRenderer spriteRenderer;

    public virtual void InicializarUnidade()
    {
        currentHP = maxHP;
        currentMP = maxMP;
    }

    public virtual bool TakeDamage(int finalDamage)
    {
        currentHP -= finalDamage;
        if (currentHP <= 0) { currentHP = 0; return true; }
        return false;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }

    // Animação de morte (Fade Out)
    public IEnumerator FadeOut()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        gameObject.SetActive(false);
    }
}