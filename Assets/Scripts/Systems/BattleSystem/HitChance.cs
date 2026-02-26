using UnityEngine;

public static class HitChance
{
    public static float CalculateHitChance(int enemyAccuracy, int playerAgility, float baseHitChance = 0.75f, float step = 0.02f, float min = 0.1f, float max = 0.95f)
    {
        float chance = baseHitChance + (enemyAccuracy - playerAgility) * step;
        return Mathf.Clamp(chance, min, max);
    }
}