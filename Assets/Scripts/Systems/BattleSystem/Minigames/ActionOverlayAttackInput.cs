using UnityEngine;
using UnityEngine.EventSystems;

public class ActionOverlayAttackInput : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("Bônus base se você não quiser depender de weakpoint no clique inicial.")]
    public float bonusBase = 1.0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Só permite iniciar ataque no estado correto
        if (BattleSystem.Instance == null) return;
        if (BattleSystem.Instance.state != BattleState.BUSY) return;

        // NÃO mude para ENEMYTURN aqui. O turno deve mudar quando o minigame finalizar!
        // BattleSystem.Instance.state = BattleState.ENEMYTURN;  <-- remova essa ideia do input

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(eventData.position);

        // Se você quiser manter o bônus por clique perto de weakpoint, dá pra calcular aqui,
        // mas como o slashing agora detecta weakpoints pela linha, o bônus inicial pode ser 1.
        AttackManager.Instance.IniciarSequenciaDeAtaque(bonusBase, mousePosWorld);
    }
}