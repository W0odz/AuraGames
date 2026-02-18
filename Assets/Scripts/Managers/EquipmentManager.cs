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

    public void Equip(DadosItem novoItem)
    {
        // 1. Descobrir o índice baseado no enum do item
        int slotIndex = (int)novoItem.slotOndeEquipa;

        // 2. Se já houver um item equipado nesse slot, devolve ele para a mochila
        if (currentEquipment[slotIndex] != null)
        {
            InventoryManager.Instance.AdicionarItem(currentEquipment[slotIndex], 1);
        }

        // 3. Coloca o novo item no slot correto
        currentEquipment[slotIndex] = novoItem;

        // 4. REMOVE da lista do inventário (Mochila)
        InventoryManager.Instance.RemoverItem(novoItem, 1);

        // 5. Atualiza tudo (UI e Atributos)
        onEquipmentChanged?.Invoke();
        InventoryUIManager.Instance.UpdateAll();
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            // 1. Pega o item que está saindo
            DadosItem itemSaindo = currentEquipment[slotIndex];

            // 2. Adiciona ele de volta à mochila
            InventoryManager.Instance.AdicionarItem(itemSaindo, 1);

            // 3. Limpa o slot de equipamento
            currentEquipment[slotIndex] = null;

            // 4. Atualiza as interfaces
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

            if (arma.tipoDeDano == TipoAtaque.Perfurante)
                AttackManager.Instance.piercing.Iniciar(arma);
            else
                AttackManager.Instance.slashing.Iniciar(arma);
        }
    }
}