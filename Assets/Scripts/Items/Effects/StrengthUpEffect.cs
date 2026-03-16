using UnityEngine;

[CreateAssetMenu(fileName = "Efeito_AumentarForca", menuName = "Items/Efeitos/Aumentar Força")]
public class StrengthUpEffect : ItemEffect
{
    [Tooltip("Quantos turnos o buff dura")]
    public int turnos = 3;

    [Tooltip("Quantos stacks aplicar (cada stack = +10 força)")]
    public int stacks = 1;

    public override void Execute(GameObject playerGO)
    {
        PlayerUnit player = PlayerUnit.Instance;
        if (player == null)
        {
            Debug.LogWarning("[StrengthUpEffect] PlayerUnit.Instance não encontrado!");
            return;
        }

        player.ApplyDebuff(DebuffType.StrengthUp, turnos, stacks);
        Debug.Log($"[StrengthUpEffect] Força aumentada por {turnos} turnos.");
    }
}