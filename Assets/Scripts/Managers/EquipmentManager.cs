using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    // Array que representa os slots do corpo (0: Arma, 1: Armadura, etc)
    public DadosItem[] equipamentosAtuais;

    public delegate void OnEquipmentChanged();
    public OnEquipmentChanged onEquipmentChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // Isso mantém os itens equipados entre as cenas

            // Inicializa o array com o tamanho do enum de slots que criamos
            int numSlots = System.Enum.GetNames(typeof(SlotEquipamento)).Length;
            equipamentosAtuais = new DadosItem[numSlots];
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public void Equipar(DadosItem novoItem)
    {
        if (novoItem == null)
        {
            Debug.LogWarning("Tentativa de equipar um item nulo abortada.");
            return;
        }

        int slotIndex = (int)novoItem.slotOndeEquipa;

        // Se já houver algo lá, você pode implementar lógica de devolver ao inventário aqui
        equipamentosAtuais[slotIndex] = novoItem;

        Debug.Log($"Equipou {novoItem.nomeItem} no slot {novoItem.slotOndeEquipa}");

        // Avisa a UI para atualizar
        onEquipmentChanged?.Invoke();
        InventoryManager.Instance.onUpdateUI?.Invoke();

        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.InicializarUnidade();
        }

    }

    public void Desequipar(int slotIndex)
    {
        // 1. Remove o item do array (define como null)
        equipamentosAtuais[slotIndex] = null;

        // 2. Atualiza os status do jogador (remove o bônus de 1099 HP)
        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.InicializarUnidade();
        }

        // 3. Notifica a UI para apagar o ícone e o nome
        onEquipmentChanged?.Invoke();
        Debug.Log($"Slot {slotIndex} desequipado com sucesso!");
    }

    public bool IsEquipado(DadosItem item)
    {
        for (int i = 0; i < equipamentosAtuais.Length; i++)
        {
            if (equipamentosAtuais[i] == item) return true;
        }
        return false;
    }
}