using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;
    public PiercingMinigame piercing;
    public SlashingMinigame slashing;

    private void Awake() { if (Instance == null) Instance = this; }

    public void IniciarSequenciaDeAtaque()
    {
        // Pega a arma no slot 0
        DadosItem item = EquipmentManager.Instance.currentEquipment[0];

        if (item is DadosArma arma)
        {
            if (arma.tipoDeDano == TipoAtaque.Perfurante)
                piercing.Iniciar(arma);
            else
                slashing.Iniciar(arma);
        }
        else
        {
            Debug.Log("Nenhuma arma equipada para usar minigame!");
        }
    }

    public void FinalizarAtaque(float multiplicador)
    {
        Debug.Log($"Ataque finalizado com precisão de: {multiplicador}");
        // Aqui você chamaria o cálculo de dano no inimigo
        BattleHUD.Instance.MostrarMenuPrincipal();
    }
}