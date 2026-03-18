using UnityEngine;

public class BloqueioArbusto : MonoBehaviour
{
    [Tooltip("Nome exato do asset DadosArma necessário para cortar o arbusto.")]
    public string nomeArmaRequerida = "Foice";

    private bool jogadorProximo = false;

    private void Update()
    {
        if (jogadorProximo && Input.GetKeyDown(KeyCode.E))
        {
            TentarCortar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorProximo = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorProximo = false;
    }

    private void TentarCortar()
    {
        if (EquipmentManager.Instance == null) return;

        DadosArma armaAtual = EquipmentManager.Instance.currentEquipment[0] as DadosArma;

        if (armaAtual == null) return;

        if (armaAtual.name == nomeArmaRequerida)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("[Arbusto] Arma incorreta. Necessária: " + nomeArmaRequerida);
        }
    }
}