using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;

    [Header("Referências de Minigames")]
    // Renomeado para 'slashing' e 'piercing' para o EquipmentManager encontrar
    public SlashingMinigame slashing;
    public PiercingMinigame piercing;

    [Header("Dados da Luta")]
    public DadosArma armaAtual;

    [Header("Mecanica de Desvio")]
    public float raioDeErro = 2.0f; // O tamanho da "zona de acerto" ao redor do clique
    private Vector2 posicaoDoClique;
    private float multiplicadorPontoFraco = 1f;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void IniciarSequenciaDeAtaque(float bonus, Vector2 coordenadaClique)
    {
        if (piercing != null && piercing.gameObject.activeInHierarchy) return;

        multiplicadorPontoFraco = bonus;
        posicaoDoClique = coordenadaClique; // Salva o ponto original

        if (armaAtual == null) return;

        if (piercing != null)
        {
            piercing.gameObject.SetActive(true);
            piercing.Iniciar(armaAtual, posicaoDoClique);
        }
    }

    public void FinalizarAtaque(bool qteSucesso)
    {
        Vector2 pontoFinalDeAcerto = posicaoDoClique;

        if (!qteSucesso)
        {
            // Cria um ponto aleatorio dentro de um circulo usando o raio de erro
            pontoFinalDeAcerto = posicaoDoClique + UnityEngine.Random.insideUnitCircle * raioDeErro;
            Debug.Log("Falhou no QTE. O ataque desviou para: " + pontoFinalDeAcerto);
        }

        // Pega o colisor do corpo do monstro para checar se o ponto caiu dentro dele
        Collider2D colisorMonstro = BattleSystem.Instance.enemyUnit.GetComponent<Collider2D>();
        bool acertouCorpo = false;

        if (colisorMonstro != null)
        {
            // Verifica se o ponto bidimensional final esta tocando no colisor
            acertouCorpo = colisorMonstro.OverlapPoint(pontoFinalDeAcerto);
        }
        else
        {
            Debug.LogWarning("O inimigo nao tem um Collider2D. Assumindo acerto por padrao.");
            acertouCorpo = true;
        }

        int danoFinal = 0;

        if (acertouCorpo)
        {
            int forcaBase = BattleSystem.Instance.playerUnit.strength;
            danoFinal = Mathf.RoundToInt(forcaBase * multiplicadorPontoFraco);

            Debug.Log("O ataque acertou o monstro! Dano: " + danoFinal);
            if (BattleSystem.Instance.dialogueText != null)
                BattleSystem.Instance.dialogueText.text = "Voce causou " + danoFinal + " de dano!";
        }
        else
        {
            Debug.Log("O ataque desviou e errou o monstro completamente!");
            if (BattleSystem.Instance.dialogueText != null)
                BattleSystem.Instance.dialogueText.text = "O ataque errou o alvo!";
        }

        bool isDead = false;

        // So chama a funcao de tomar dano se o valor for maior que zero
        if (danoFinal > 0)
        {
            isDead = BattleSystem.Instance.enemyUnit.TakeDamage(danoFinal);
            BattleSystem.Instance.enemyHUD.UpdateHP(BattleSystem.Instance.enemyUnit.currentHP);
        }

        if (piercing != null) piercing.gameObject.SetActive(false);

        if (isDead) BattleSystem.Instance.StartCoroutine("EndBattle");
        else BattleSystem.Instance.StartCoroutine("EnemyTurn");
    }
}