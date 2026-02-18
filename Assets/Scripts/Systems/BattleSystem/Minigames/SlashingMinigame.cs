using UnityEngine;

public class SlashingMinigame : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LayerMask layerPontosFracos;
    private Vector2 startPoint;
    private float maxRadius;
    private bool isAiming = false;

    public void Iniciar(DadosArma arma)
    {
        // Aqui usamos o valor da arma como o raio do limite invisível
        maxRadius = arma.limiteDeTinta > 0 ? arma.limiteDeTinta : 3f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        // CORREÇÃO: Usamos GetMouseButton(0) em vez de GetButton(0)
        if (Input.GetMouseButtonDown(0))
        {
            startPoint = GetMouseWorldPos();
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.enabled = true;
            isAiming = true;
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = GetMouseWorldPos();
            Vector2 direction = (currentMousePos - startPoint).normalized;
            float distance = Vector2.Distance(startPoint, currentMousePos);

            // TRAVA DO RAIO INVISÍVEL
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

        // Verifica se a linha atravessou a Layer de Pontos Fracos
        RaycastHit2D hit = Physics2D.Linecast(startPoint, endPoint, layerPontosFracos);

        float mult = (hit.collider != null) ? 2.0f : 0.8f;

        lineRenderer.enabled = false;
        gameObject.SetActive(false);
        AttackManager.Instance.FinalizarAtaque(mult);
    }

    private Vector2 GetMouseWorldPos() => Camera.main.ScreenToWorldPoint(Input.mousePosition);
}