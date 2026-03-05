using System.Collections.Generic;
using UnityEngine;

public class CollectionLogManager : MonoBehaviour
{
    public static CollectionLogManager Instance;
    public GameObject logPrefab;
    public Transform logContainer;

    // Guarda os itens que já apareceram no log ao menos uma vez
    private HashSet<DadosItem> itensRegistrados = new HashSet<DadosItem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddLog(DadosItem item, int qtd)
    {
        // Se o item já foi registrado antes, ignora — desequipar não conta
        if (itensRegistrados.Contains(item)) return;

        itensRegistrados.Add(item);

        // Diminui opacidade dos logs anteriores
        foreach (Transform child in logContainer)
        {
            CollectionLogEntry entryAntiga = child.GetComponent<CollectionLogEntry>();
            if (entryAntiga != null)
                entryAntiga.DiminuirOpacidade(0.4f);
        }

        // Instancia o novo log
        GameObject newEntry = Instantiate(logPrefab, logContainer);
        newEntry.GetComponent<CollectionLogEntry>().Setup(item, qtd);
        newEntry.transform.SetAsLastSibling();
    }
}