using UnityEngine;

public class PiercingMinigame : MonoBehaviour
{
    public RectTransform ringUI; // O aro que diminui
    public float perfectScale = 1f; // Onde ele deve acertar
    public float margin = 0.2f; // Margem de erro

    private bool isActive;
    private float currentScale;
    private float speed;

    public void Iniciar(DadosArma arma)
    {
        isActive = true;
        currentScale = 3f; // Começa grande
        speed = arma.velocidadeDoAro;
        ringUI.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;

        currentScale -= Time.deltaTime * speed;
        ringUI.localScale = new Vector3(currentScale, currentScale, 1);

        if (Input.GetButtonDown("Fire1")) ValidarAcerto();
        if (currentScale <= 0.5f) Fail();
    }

    void ValidarAcerto()
    {
        float precisao = 1f - Mathf.Abs(currentScale - perfectScale);
        float mult = (precisao > (1 - margin)) ? 2f : 1f; // Dobra se for perfeito
        Finalizar(mult);
    }

    void Fail() => Finalizar(0.5f);

    void Finalizar(float mult)
    {
        isActive = false;
        ringUI.gameObject.SetActive(false);
        AttackManager.Instance.FinalizarAtaque(mult);
    }
}