using UnityEngine;

[CreateAssetMenu(fileName = "Novo Efeito de Cura", menuName = "Items/Efeitos/Cura")]
public class HealEffect : ItemEffect
{
    public int quantidadeCura = 50; // O valor que você definiu

    public override void Execute(GameObject player)
    {
        // Aqui você acessa o seu script de vida do jogador
        PlayerUnit stats = player.GetComponent<PlayerUnit>();
        if (stats != null)
        {
            stats.Heal(quantidadeCura);
            Debug.Log($"Curou {quantidadeCura} de vida!");
        }
    }
}