using UnityEngine;

public class MonsterTarget : MonoBehaviour
{
    [Header("Configuração de Mira (Perfurante)")]
    public float raioDeAcerto = 0.3f;

    [Header("Bônus ao clicar em ponto fraco (Perfurante)")]
    public float bonusAoAcertarPontoFraco = 0.4f;

    [Header("Debug")]
    public bool logDistancias = true;

    void OnMouseDown()
    {
        // Só pode escolher alvo quando estiver no modo de seleção de alvo
        if (BattleSystem.Instance == null) return;
        if (BattleSystem.Instance.state != BattleState.TARGETING) return;

        if (AttackManager.Instance == null) return;
        if (AttackManager.Instance.armaAtual == null) return;

        // MonsterTarget só deve iniciar o ataque PERFURANTE
        if (AttackManager.Instance.armaAtual.tipoDeDano != TipoAtaque.Perfurante) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float bonus = 1.0f;
        bool acertouWeakpoint = false;

        // Procura weakpoints e dá "hit" se clicou perto
        WeakPoint[] pontosFracos = FindObjectsByType<WeakPoint>(FindObjectsSortMode.None);
        foreach (WeakPoint ponto in pontosFracos)
        {
            float distancia = Vector2.Distance(mousePos, ponto.transform.position);

            if (logDistancias)
                Debug.Log($"Distância do clique para o Ponto Fraco: {distancia}");

            if (distancia <= raioDeAcerto)
            {
                ponto.ReceberClique();
                acertouWeakpoint = true;
                break;
            }
        }

        if (acertouWeakpoint)
            bonus += bonusAoAcertarPontoFraco;

        // Agora sim inicia o minigame do perfurante com o clique escolhido
        AttackManager.Instance.IniciarSequenciaDeAtaque(bonus, mousePos);
    }
}