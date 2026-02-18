using UnityEngine;

public class CollectionLogManager : MonoBehaviour
{
    public static CollectionLogManager Instance;
    public GameObject logPrefab; // O prefab com o script acima
    public Transform logContainer; // Um objeto com Vertical Layout Group

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddLog(DadosItem item, int qtd)
    {
        // 1. Percorre todos os logs que já estão na tela e diminui a opacidade deles
        foreach (Transform child in logContainer)
        {
            CollectionLogEntry entryAntiga = child.GetComponent<CollectionLogEntry>();
            if (entryAntiga != null)
            {
                // Define a opacidade dos antigos (ex: 40%)
                entryAntiga.DiminuirOpacidade(0.4f);
            }
        }

        // 2. Instancia o novo log (que nascerá com Alpha 1 via Setup)
        GameObject newEntry = Instantiate(logPrefab, logContainer);
        newEntry.GetComponent<CollectionLogEntry>().Setup(item, qtd);

        newEntry.transform.SetAsLastSibling();
    }
}