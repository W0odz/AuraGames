using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int playerLevel;

    public int maxHP;
    public int currentHP;

    public int maxMP;
    public int currentMP;

    public int strength;
    public int resistance;
    public int will;
    public int knowledge;
    public int luck;

    public bool isDefending = false;

    public int xpValue = 50;

    public SpriteRenderer spriteRenderer;

    // Usado pelo BattleSystem para configurar o JOGADOR
    public void SetupPlayerStats(GameManager gm)
    {
        unitName = gm.playerName;

        // Puxa todos os stats do GameManager
        playerLevel = gm.playerLevel; // Embora não usemos level na batalha ainda
        maxHP = gm.maxHP;
        maxMP = gm.maxMP;
        currentHP = gm.currentHP; // Pega o HP atual!
        currentMP = gm.currentMP;
        strength = gm.strength;
        resistance = gm.resistance;
        will = gm.will;
        knowledge = gm.knowledge;
        luck = gm.luck;
    }

    // Função para receber dano
    public bool TakeDamage(int dmg, bool isMagic)
    {
        int finalDamage = dmg;

        // Aplica a defesa correta
        if (isMagic)
        {
            finalDamage -= knowledge;
        }
        else
        {
            finalDamage -= resistance;
        }

        // Se estiver defendendo, reduz pela metade
        if (isDefending)
        {
            dmg /= 2;
        }

        // Garante que o dano seja pelo menos 1
        if (finalDamage < 1)
        {
            finalDamage = 1;
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