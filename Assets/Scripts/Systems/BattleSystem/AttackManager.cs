using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;

    [Header("Referências de Minigames")]
    public SlashingMinigame slashing;
    public PiercingMinigame piercing;

    [Header("Dados da Luta")]
    public DadosArma armaAtual;

    [Header("Mecanica de Desvio (Piercing)")]
    public float raioDeErro = 2.0f;
    private Vector2 posicaoDoClique;
    private float multiplicadorPontoFraco = 1f;

    [Header("Slashing - Dano por múltiplos pontos fracos")]
    public float slashingFirstWeakPointBonus = 0.4f;

    [Range(0.1f, 0.95f)]
    public float slashingBonusDecay = 0.6f;

    public float slashingMaxMultiplier = 2.0f;

    [Header("Slashing - validação de acerto no corpo (Collider2D do monstro é Trigger)")]
    [Tooltip("Layer do corpo do inimigo (recomendado). Coloque o collider trigger do monstro nessa layer.")]
    public LayerMask enemyBodyLayer;

    [Tooltip("Se true, usa Physics2D.Linecast para validar acerto (recomendado p/ Trigger).")]
    public bool slashingUseLinecast = true;

    [Range(2, 64)]
    public int slashingHitSamples = 16;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void IniciarSequenciaDeAtaque(float bonus, Vector2 coordenadaClique)
    {
        if (piercing != null && piercing.gameObject.activeInHierarchy) return;
        if (slashing != null && slashing.gameObject.activeInHierarchy) return;

        multiplicadorPontoFraco = bonus;
        posicaoDoClique = coordenadaClique;

        if (armaAtual == null) return;

        switch (armaAtual.tipoDeDano)
        {
            case TipoAtaque.Cortante:
                if (slashing != null)
                {
                    slashing.gameObject.SetActive(true);
                    slashing.Iniciar(armaAtual);
                }
                break;

            case TipoAtaque.Perfurante:
            default:
                if (piercing != null)
                {
                    piercing.gameObject.SetActive(true);
                    piercing.Iniciar(armaAtual, posicaoDoClique);
                }
                break;
        }
    }

    public void FinalizarAtaque(bool qteSucesso)
    {
        Vector2 pontoFinalDeAcerto = posicaoDoClique;

        if (!qteSucesso)
            pontoFinalDeAcerto = posicaoDoClique + UnityEngine.Random.insideUnitCircle * raioDeErro;

        Collider2D colisorMonstro = BattleSystem.Instance.enemyUnit.GetComponentInParent<Collider2D>();
        bool acertouCorpo = colisorMonstro != null ? colisorMonstro.OverlapPoint(pontoFinalDeAcerto) : true;

        AplicarDano(acertouCorpo, multiplicadorPontoFraco);

        if (piercing != null) piercing.gameObject.SetActive(false);
    }

    public void FinalizarAtaqueSlashing(bool sucesso, int weakPointsHit, float precisao, Vector2 slashStart, Vector2 slashEnd)
    {
        bool acertouCorpo = false;

        if (sucesso)
            acertouCorpo = SlashSegmentHitsEnemyBody(slashStart, slashEnd);

        float multWeakpoints = CalcularMultiplicadorSlashing(weakPointsHit);
        float multiplicadorFinal = multiplicadorPontoFraco * multWeakpoints;

        AplicarDano(acertouCorpo, multiplicadorFinal);

        if (slashing != null) slashing.gameObject.SetActive(false);
    }

    bool SlashSegmentHitsEnemyBody(Vector2 a, Vector2 b)
    {
        if (slashingUseLinecast)
        {
            RaycastHit2D hit = (enemyBodyLayer.value != 0)
                ? Physics2D.Linecast(a, b, enemyBodyLayer)
                : Physics2D.Linecast(a, b);

            return hit.collider != null;
        }

        // fallback
        Collider2D colisorMonstro = BattleSystem.Instance.enemyUnit.GetComponentInParent<Collider2D>();
        if (colisorMonstro == null) return true;

        int steps = Mathf.Max(2, slashingHitSamples);
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)(steps - 1);
            Vector2 p = Vector2.Lerp(a, b, t);

            if (colisorMonstro.OverlapPoint(p))
                return true;
        }

        return false;
    }

    float CalcularMultiplicadorSlashing(int weakPointsHit)
    {
        if (weakPointsHit <= 0) return 1f;

        float totalBonus = 0f;
        float bonusAtual = slashingFirstWeakPointBonus;

        for (int i = 0; i < weakPointsHit; i++)
        {
            totalBonus += bonusAtual;
            bonusAtual *= slashingBonusDecay;
        }

        return Mathf.Min(1f + totalBonus, slashingMaxMultiplier);
    }

    void AplicarDano(bool acertouCorpo, float multiplicadorFinal)
    {
        int danoFinal = 0;

        if (acertouCorpo)
        {
            int forcaBase = BattleSystem.Instance.playerUnit.strength;
            danoFinal = Mathf.RoundToInt(forcaBase * multiplicadorFinal);

            if (BattleSystem.Instance.dialogueText != null)
                BattleSystem.Instance.dialogueText.text = "Voce causou " + danoFinal + " de dano!";
        }
        else
        {
            if (BattleSystem.Instance.dialogueText != null)
                BattleSystem.Instance.dialogueText.text = "O ataque errou o alvo!";
        }

        if (danoFinal > 0)
        {
            bool isDead = BattleSystem.Instance.enemyUnit.TakeDamage(danoFinal);
            BattleSystem.Instance.enemyHUD.UpdateHP(BattleSystem.Instance.enemyUnit.currentHP);

            if (isDead) BattleSystem.Instance.StartCoroutine("EndBattle");
            else BattleSystem.Instance.StartCoroutine("EnemyTurn");
        }
        else
        {
            BattleSystem.Instance.StartCoroutine("EnemyTurn");
        }
    }
}