using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    [Header("Identidade")]
    public string unitName;
    public int playerLevel;

    [Header("Recursos")]
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;

    [Header("Atributos")]
    public int strength;
    public int resistance;
    public int will;
    public int knowledge;
    public int speed;
    public int luck;

    [Header("Estado")]
    public bool isDefending = false;
    public int xpValue = 0; // XP que o inimigo dá ao morrer

    public SpriteRenderer spriteRenderer;

    // Puxa os dados do GameManager para preencher a ficha do Jogador
    public void SetupPlayerStats(GameManager gm)
    {
        unitName = gm.playerName;
        playerLevel = gm.playerLevel;

        maxHP = gm.maxHP;
        maxMP = gm.maxMP;
        currentHP = gm.currentHP;
        currentMP = gm.currentMP;

        strength = gm.strength;
        resistance = gm.resistance;
        will = gm.will;
        knowledge = gm.knowledge;
        speed = gm.speed;
        luck = gm.luck;

        xpValue = 0; // Jogador não dá XP

        // Segurança: Se a vida vier zerada, cura.
        if (currentHP <= 0)
        {
            currentHP = maxHP;
            gm.currentHP = maxHP;
        }
    }

    // Recebe o dano já calculado (Ataque - Defesa) e aplica
    public bool TakeDamage(int finalDamage)
    {
        currentHP -= finalDamage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            return true; // Morreu
        }
        else
        {
            return false; // Vivo
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