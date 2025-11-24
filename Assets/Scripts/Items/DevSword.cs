using UnityEngine;

public class DevSword : MonoBehaviour
{
    [Header("Configurações")]
    public string itemID = "DevSword_Unique_01"; // ID único para este item
    public int godModeValue = 999; // O valor para todos os stats
    public string feedbackMessage = "VOCÊ OBTEVE O PODER DOS DEUSES!";

    void Start()
    {
        // Se este item já está na lista de coletados, se autodestrói imediatamente
        if (GameManager.instance.collectedItemIDs.Contains(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verifica se a física detectou QUALQUER coisa
        Debug.Log("Alguém tocou na espada! Foi: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("É o jogador! Aplicando efeitos...");
            ApplyGodMode();

            GameManager.instance.collectedItemIDs.Add(itemID);
            Destroy(gameObject);
        }
        else
        {
            // 2. Avisa se a tag estiver errada
            Debug.LogWarning("O objeto colidiu, mas a TAG não é 'Player'. A tag é: " + other.tag);
        }
    }

    void ApplyGodMode()
    {
        // Acessa o GameManager (onde os stats reais vivem)
        GameManager gm = GameManager.instance;

        // --- REESCREVE TUDO PARA 999 ---

        // 1. Status de Combate
        gm.strength = godModeValue;
        gm.speed = godModeValue;
        gm.resistance = godModeValue;
        gm.will = godModeValue;
        gm.knowledge = godModeValue;
        gm.luck = godModeValue; // Crítico garantido (se a lógica for <= luck)

        // 2. Vida e Mana (Máximos e Atuais)
        gm.maxHP = godModeValue;
        gm.maxMP = godModeValue;

        // Cura total imediata
        gm.currentHP = godModeValue;
        gm.currentMP = godModeValue;

        // 3. Nível (Opcional: Deixa nível 99 também?)
        gm.playerLevel = 99;

        // Feedback no Console (ou use seu sistema de mensagens se tiver um)
        Debug.Log(feedbackMessage);
    }
}