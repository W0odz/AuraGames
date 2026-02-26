using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ActionOverlayAttackInput : MonoBehaviour, IPointerDownHandler
{
    public float bonusBase = 1.0f;
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true; // ou false, dependendo do seu fluxo
    }

    public void SetEnabled(bool enabled)
    {
        if (img != null) img.raycastTarget = enabled;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[ActionOverlayAttackInput] Clique recebido. BattleSystem? {(BattleSystem.Instance!=null)} state={(BattleSystem.Instance!=null ? BattleSystem.Instance.state.ToString() : "NULL")}");


        // Só permite iniciar ataque no estado correto
        if (BattleSystem.Instance == null) return;
        if (BattleSystem.Instance.state != BattleState.BUSY) return;

        // NÃO mude para ENEMYTURN aqui. O turno deve mudar quando o minigame finalizar!
        // BattleSystem.Instance.state = BattleState.ENEMYTURN;  <-- remova essa ideia do input

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(eventData.position);

        // Se você quiser manter o bônus por clique perto de weakpoint, dá pra calcular aqui,
        // mas como o slashing agora detecta weakpoints pela linha, o bônus inicial pode ser 1.
        Debug.Log("[ActionOverlayAttackInput] Clique no overlay, iniciando ataque");

        AttackManager.Instance.IniciarSequenciaDeAtaque(bonusBase, mousePosWorld);
    }
}