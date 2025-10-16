using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BattleLogManager : MonoBehaviour
{
    public GameObject logTextPrefab; // O prefab do nosso texto
    public int maxMessages = 3;      // O limite de mensagens

    // Lista para guardar as mensagens ativas na tela
    private List<TextMeshProUGUI> messageList = new List<TextMeshProUGUI>();

    // Função pública para adicionar uma nova mensagem ao log
    public void AddLogMessage(string message)
    {
        // Se já atingimos o limite, remove a mensagem mais antiga
        if (messageList.Count >= maxMessages)
        {
            // Pega a primeira da lista (a mais antiga)
            TextMeshProUGUI oldestMessage = messageList[0];
            messageList.RemoveAt(0);
            Destroy(oldestMessage.gameObject);
        }

        // Cria uma nova instância do prefab de texto
        GameObject newTextGO = Instantiate(logTextPrefab, transform);
        TextMeshProUGUI newText = newTextGO.GetComponent<TextMeshProUGUI>();

        newText.text = message;
        messageList.Add(newText);

        UpdateMessageOpacity();
    }

    // Atualiza a opacidade de todas as mensagens
    private void UpdateMessageOpacity()
    {
        // A opacidade diminui para as mensagens mais antigas
        float startingOpacity = 1f;
        float opacityStep = 0.4f; // Quanto cada mensagem antiga perde de opacidade

        // Itera da mais recente para a mais antiga
        for (int i = messageList.Count - 1; i >= 0; i--)
        {
            TextMeshProUGUI msg = messageList[i];

            // Calcula a opacidade atual baseada na sua "idade"
            float targetAlpha = startingOpacity - (messageList.Count - 1 - i) * opacityStep;

            // Garante que o alpha não seja menor que zero
            if (targetAlpha < 0) targetAlpha = 0;

            // Aplica a nova cor com o alpha modificado
            msg.color = new Color(msg.color.r, msg.color.g, msg.color.b, targetAlpha);
        }
    }
}