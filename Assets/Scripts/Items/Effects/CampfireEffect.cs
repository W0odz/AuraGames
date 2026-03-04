using System.Collections.Generic;
using System.IO;
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
        // TODO: Descomentar quando SaveData.cs e SlotSaveData estiverem criados
        // Salvar(player);

        // 2. Restaurar a Força de Vontade
        player.RestaurarForcaDeVontade();

        Debug.Log("[Fogueira] Força de Vontade restaurada! (Save desativado temporariamente)");
    }

    // ─────────────────────────────────────────────
    //  SAVE — descomentar após criar SaveData.cs
    // ─────────────────────────────────────────────

    // void Salvar(PlayerUnit player)
    // {
    //     var save = new SaveData();
    //
    //     save.sceneName   = SceneManager.GetActiveScene().name;
    //     save.posX        = player.transform.position.x;
    //     save.posY        = player.transform.position.y;
    //
    //     save.currentHP   = player.currentHP;
    //     save.maxHP       = player.maxHP;
    //     save.currentMP   = player.currentMP;
    //     save.maxMP       = player.maxMP;
    //     save.currentXP   = player.currentXP;
    //     save.playerLevel = player.playerLevel;
    //
    //     save.temForcaDeVontade = true;
    //
    //     save.inventario = new List<SlotSaveData>();
    //     if (InventoryManager.Instance != null)
    //     {
    //         foreach (var slot in InventoryManager.Instance.listaItens)
    //         {
    //             if (slot.item != null)
    //                 save.inventario.Add(new SlotSaveData
    //                 {
    //                     nomeItem   = slot.item.nomeItem,
    //                     quantidade = slot.quantidade
    //                 });
    //         }
    //     }
    //
    //     save.inimigosDerrrotados = GameManager.Instance != null
    //         ? GameManager.Instance.GetDefeatedEnemyIDs()
    //         : new List<string>();
    //
    //     string json = JsonUtility.ToJson(save, prettyPrint: true);
    //     File.WriteAllText(SavePath, json);
    //     Debug.Log($"[SaveSystem] Salvo em: {SavePath}");
    // }
    //
    // static string SavePath =>
    //     Path.Combine(Application.persistentDataPath, "save.json");
}