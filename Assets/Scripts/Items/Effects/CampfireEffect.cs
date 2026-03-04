using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Efeito do item Fogueira.
/// Quando executado (uso do item):
///   1. Salva o jogo em disco (JSON via Application.persistentDataPath)
///   2. Restaura a Força de Vontade do jogador
/// </summary>
[CreateAssetMenu(fileName = "Efeito_Fogueira", menuName = "Items/Efeitos/Usar Fogueira")]
public class UsarFogueiraEffect : ItemEffect
{
    public override void Execute(GameObject playerGO)
    {
        PlayerUnit player = PlayerUnit.Instance;

        if (player == null)
        {
            Debug.LogWarning("[UsarFogueiraEffect] PlayerUnit.Instance não encontrado!");
            return;
        }

        // 1. Salvar o jogo
        Salvar(player);

        // 2. Restaurar a Força de Vontade
        player.RestaurarForcaDeVontade();

        Debug.Log("[Fogueira] Jogo salvo e Força de Vontade restaurada!");
    }

    // ─────────────────────────────────────────────
    //  SAVE
    // ─────────────────────────────────────────────

    void Salvar(PlayerUnit player)
    {
        var save = new SaveData();

        // Posição e cena atuais
        save.sceneName = SceneManager.GetActiveScene().name;
        save.posX = player.transform.position.x;
        save.posY = player.transform.position.y;

        // Stats do jogador
        save.currentHP = player.currentHP;
        save.maxHP = player.maxHP;
        save.currentMP = player.currentMP;
        save.maxMP = player.maxMP;
        save.currentXP = player.currentXP;
        save.playerLevel = player.playerLevel;

        // Força de Vontade (salva como TRUE porque acabou de restaurar)
        save.temForcaDeVontade = true;

        // Inventário — salva por nome (SO não é serializável por referência)
        save.inventario = new List<SlotSaveData>();
        if (InventoryManager.Instance != null)
        {
            foreach (var slot in InventoryManager.Instance.listaItens)
            {
                if (slot.item != null)
                    save.inventario.Add(new SlotSaveData
                    {
                        nomeItem = slot.item.nomeItem,
                        quantidade = slot.quantidade
                    });
            }
        }

        // Inimigos derrotados
        save.inimigosDerrrotados = GameManager.Instance != null
            ? GameManager.Instance.GetDefeatedEnemyIDs()
            : new List<string>();

        // Escreve em disco
        string json = JsonUtility.ToJson(save, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Salvo em: {SavePath}");
    }

    static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");
}