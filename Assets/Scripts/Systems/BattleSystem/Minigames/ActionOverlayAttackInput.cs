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
        if (img != null) img.raycastTarget = false; // começa desligado
    }

    public void SetEnabled(bool enabled)
    {
        if (img != null) img.raycastTarget = enabled;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (BattleSystem.Instance == null) return;
        if (AttackManager.Instance == null) return;

        // Só no momento de selecionar (clique inicial)
        if (BattleSystem.Instance.state != BattleState.TARGETING) return;

        // Overlay só serve para iniciar o CORTANTE
        if (AttackManager.Instance.armaAtual == null) return;
        if (AttackManager.Instance.armaAtual.tipoDeDano != TipoAtaque.Cortante) return;

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        AttackManager.Instance.IniciarSequenciaDeAtaque(bonusBase, mousePosWorld);
    }
}