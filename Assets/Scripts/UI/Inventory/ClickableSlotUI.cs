using UnityEngine;
using UnityEngine.UI;

public class SlotClicavelUI : MonoBehaviour
{
    public SlotEquipamento tipoDesteSlot;

    void Start()
    {
        // Adiciona a função de clique automaticamente se houver um botão
        GetComponent<Button>().onClick.AddListener(() => {
            InventoryUIManager.Instance.SelecionarSlotParaEquipar((int)tipoDesteSlot);
        });
    }
}