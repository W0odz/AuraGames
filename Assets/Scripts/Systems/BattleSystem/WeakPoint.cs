using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [Header("Configurações do Ponto")]
    public float multiplicadorDano = 1.5f;

    //[Header("Feedback Visual")]
    //public GameObject efeitoAcerto;

    // Função para calcular o dano, independente da arma
    public float CalcularDanoRecebido(float danoBaseDaArma)
    {
        float danoFinal = danoBaseDaArma * multiplicadorDano;

        //ExecutarFeedback();

        Debug.Log($"Ponto Fraco Atingido! Dano: {danoFinal}");
        return danoFinal;
    }

    //private void ExecutarFeedback()
    //{
    //    if (efeitoAcerto != null)
    //        Instantiate(efeitoAcerto, transform.position, Quaternion.identity);
    //}
}