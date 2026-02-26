using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Body Part Definition")]
public class BodyPartDefinition : ScriptableObject
{
    public BodyPartType part;
    [Range(0.1f, 3f)] public float damageMultiplier = 1f;

    [Header("Debuff")]
    public DebuffType debuff;
    public int debuffTurns = 2;
    public int debuffStacks = 1;
}