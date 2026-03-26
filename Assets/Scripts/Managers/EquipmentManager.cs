using UnityEngine;

public enum SlotEquipamento { Weapon, Helmet, Chestplate, Gloves, Legs, Nenhum }

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    // Array para os 5 equipamentos frontais da imagem
    public DadosItem[] currentEquipment = new DadosItem[5];

    public delegate void OnEquipmentChanged();
    public OnEquipmentChanged onEquipmentChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void AplicarBonusItem(DadosItem item, int sinal)
    {
        if (item == null || GameManager.Instance == null) return;
        GameManager.Instance.strength += sinal * item.bonusStrength;
        GameManager.Instance.maxHP += sinal * item.bonusMaxHP;
        GameManager.Instance.resistance += sinal * item.bonusResistance;
        // Garante que o HP atual não ultrapasse o novo maxHP após remover um item
        GameManager.Instance.currentHP = Mathf.Min(GameManager.Instance.currentHP, GameManager.Instance.maxHP);
    }

    public void Equip(DadosItem novoItem)
    {
        // 1. Descobrir o índice baseado no enum do item
        int slotIndex = (int)novoItem.slotOndeEquipa;

        // 2. Se já houver um item equipado nesse slot, subtrai os bônus e devolve à mochila
        if (currentEquipment[slotIndex] != null)
        {
            AplicarBonusItem(currentEquipment[slotIndex], -1);
            InventoryManager.Instance.AdicionarItem(currentEquipment[slotIndex], 1);
        }

        // 3. Coloca o novo item no slot correto e aplica seus bônus
        currentEquipment[slotIndex] = novoItem;
        AplicarBonusItem(novoItem, +1);

        // 4. REMOVE da lista do inventário (Mochila)
        InventoryManager.Instance.RemoverItem(novoItem, 1);

        // 5. Atualiza tudo (UI e Atributos)
        onEquipmentChanged?.Invoke();
        InventoryUIManager.Instance.UpdateAll();
    }

    public void Unequip(int slotIndex)
    {
        // Slot de arma (0) nunca pode ser desequipado diretamente.
        // Para trocar de arma, equipe uma nova — ela substitui automaticamente.
        if (slotIndex == (int)SlotEquipamento.Weapon)
        {
            Debug.LogWarning("[EquipmentManager] A arma não pode ser desequipada. Equipe outra arma para substituí-la.");
            return;
        }

        if (currentEquipment[slotIndex] != null)
        {
            DadosItem itemSaindo = currentEquipment[slotIndex];
            AplicarBonusItem(itemSaindo, -1);
            InventoryManager.Instance.AdicionarItem(itemSaindo, 1);
            currentEquipment[slotIndex] = null;
            onEquipmentChanged?.Invoke();
            InventoryUIManager.Instance.UpdateAll();
        }
    }

    public void IniciarAtaque()
    {
        // Pega o item que está no slot de Arma (índice 0)
        DadosItem itemEquipado = currentEquipment[0];

        // Tenta transformar o item genérico em uma Arma
        if (itemEquipado is DadosArma arma)
        {
            // Agora você tem acesso aos dados específicos!
            Debug.Log("Iniciando minigame de: " + arma.tipoDeDano);

            // Trocamos o currentEquipment[0] por 1.0f (que é o multiplicador de dano normal)
            if (arma.tipoDeDano == TipoAtaque.Perfurante)
                AttackManager.Instance.IniciarSequenciaDeAtaque(1.0f, new Vector2(Screen.width / 2f, Screen.height / 2f));
            else
                AttackManager.Instance.IniciarSequenciaDeAtaque(1.0f, new Vector2(Screen.width / 2f, Screen.height / 2f));
        }
    }
}