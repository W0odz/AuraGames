using UnityEngine;

public class SlashingMinigame : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private Vector2 startPoint;
    private float maxRadius;
    private bool isAiming = false;
    private bool isActive = false;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Iniciar(DadosArma arma)
    {
        maxRadius = arma.limiteDeTinta > 0 ? arma.limiteDeTinta : 3f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        isActive = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            startPoint = GetMouseWorldPos();
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, startPoint); // Evita glitch visual no primeiro frame
            lineRenderer.enabled = true;
            isAiming = true;
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = GetMouseWorldPos();
            Vector2 direction = (currentMousePos - startPoint).normalized;
            float distance = Vector2.Distance(startPoint, currentMousePos);

            // Trava do raio limite
            float clampedDist = Mathf.Min(distance, maxRadius);
            Vector2 endPoint = startPoint + (direction * clampedDist);

            lineRenderer.SetPosition(1, endPoint);
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            ValidarCorte();
        }
    }

    void ValidarCorte()
    {
        isAiming = false;
        Vector2 endPoint = lineRenderer.GetPosition(1);

        // Calcula a distancia que o jogador arrastou o mouse
        float distanciaCortada = Vector2.Distance(startPoint, endPoint);

        // Define a precisão comparando o quanto ele cortou com o máximo permitido
        float precisao = distanciaCortada / maxRadius;

        // Se ele desenhou uma linha que tem pelo menos metade do tamanho máximo, é um sucesso
        bool sucesso = precisao > 0.5f;

        lineRenderer.enabled = false;
        Finalizar(sucesso);
    }

    private Vector2 GetMouseWorldPos() => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    void Finalizar(bool sucesso)
    {
        isActive = false;
        gameObject.SetActive(false);

        // Devolve o resultado para o gestor terminar o calculo de desvio e dano
        AttackManager.Instance.FinalizarAtaque(sucesso);
    }
}