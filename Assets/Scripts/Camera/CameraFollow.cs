using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    // Crie um campo público para arrastar o jogador aqui
    public Transform target;

    // LateUpdate é chamado depois que todos os Updates (incluindo o do jogador)
    // foram executados. É o lugar ideal para mover a câmera.
    void LateUpdate()
    {
        // Se o alvo (jogador) existir
        if (target != null)
        {
            // Mova a câmera para a posição X e Y do jogador,
            // mas mantenha a posição Z original da câmera
            // (que é -10, por padrão, para ela ficar "na frente" dos sprites)
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }
    }
}