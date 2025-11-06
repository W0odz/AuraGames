// SaveSystem.cs
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    // Define o caminho do arquivo de save
    private static string GetSavePath(int slot)
    {
        // Application.persistentDataPath é uma pasta segura
        // que a Unity cria em qualquer dispositivo (PC, Mobile, etc)
        return Path.Combine(Application.persistentDataPath, "save" + slot + ".json");
    }

    // Função para salvar o jogo
    public static void SaveGame(GameData data, int slot)
    {
        string path = GetSavePath(slot);

        // Converte o objeto GameData para um texto JSON
        string json = JsonUtility.ToJson(data, true);

        // Escreve o texto no arquivo
        File.WriteAllText(path, json);
        Debug.Log("Jogo salvo no Slot " + slot);
    }

    public static void EraseGame(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save do Slot " + slot + " apagado.");
        }
    }

    // Função para carregar o jogo
    public static GameData LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        // Se o arquivo existir...
        if (File.Exists(path))
        {
            // Lê o texto do arquivo
            string json = File.ReadAllText(path);

            // Converte o JSON de volta para o objeto GameData
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }
        else
        {
            // Se não existir, retorna nulo (sem save)
            return null;
        }
    }

    // Função para checar se o arquivo existe
    public static bool SaveFileExists(int slot)
    {
        string path = GetSavePath(slot);
        return File.Exists(path);
    }
}