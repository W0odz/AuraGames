using UnityEngine;

public class SecretMessage : MonoBehaviour
{
    [Header("Condição")]
    public string forbiddenItemID; // ID da espada

    [Header("Objetos Controlados")]
    public GameObject[] textsToControl; // Arraste os dois textos aqui

    void OnEnable()
    {
        CheckCondition();
    }

    void CheckCondition()
    {
        // Verifica se o jogador TEM a espada
        bool hasSword = GameManager.Instance.collectedItemIDs.Contains(forbiddenItemID);

        foreach (GameObject obj in textsToControl)
        {
            if (obj != null)
            {
                // 1. Garante que o objeto esteja ATIVO (Visível)
                obj.SetActive(true);

                // 2. Pega o componente Animator do objeto
                Animator anim = obj.GetComponent<Animator>();

                if (anim != null)
                {
                    if (hasSword)
                    {
                        // TEM A ESPADA: Desliga a animação (fica estático)
                        anim.enabled = false;

                        // Opcional: Se a animação for de "aparecer" (fade in), 
                        // talvez você precise garantir que ele esteja totalmente visível aqui.
                        // Mas, por padrão, ele ficará no estado em que foi desenhado na cena.
                    }
                    else
                    {
                        // NÃO TEM A ESPADA: Liga a animação (toca normalmente)
                        anim.enabled = true;
                    }
                }
            }
        }
    }
}