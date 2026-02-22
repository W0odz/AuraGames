using UnityEngine;

public class SlashingMinigame : MonoBehaviour
{
    public enum LimiteModo
    {
        Comprimento,
        Tempo,
        Ambos
    }

    [Header("UI / Action Overlay")]
    public RectTransform actionOverlay;
    public Camera uiCamera;

    [Header("Mundo")]
    public Camera worldCamera;

    [Header("Referências")]
    public LineRenderer lineRenderer;

    [Header("Detecção (weakpoints sem Collider2D)")]
    public float slashThickness = 0.35f;

    [Header("Regra de sucesso")]
    [Range(0f, 1f)]
    public float minPrecisionToSucceed = 0.5f;

    [Header("Limite do desenho")]
    public LimiteModo limiteModo = LimiteModo.Ambos;
    public float maxRadiusOverride = 0f;
    public float maxDrawTime = 0.9f;
    public bool autoFinalizeOnTimeout = true;

    private Vector2 startPoint;
    private Vector2 endPoint;
    private float maxRadiusFromWeapon;
    private bool isAiming;
    private bool isActive;
    private float aimingTimer;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Iniciar(DadosArma arma)
    {
        if (worldCamera == null) worldCamera = Camera.main;

        maxRadiusFromWeapon = (arma != null && arma.limiteDeTinta > 0) ? arma.limiteDeTinta : 1.0f;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
        }

        isAiming = false;
        isActive = true;
        aimingTimer = 0f;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;
        if (worldCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsMouseInsideActionOverlay()) return;

            startPoint = GetMouseWorldPos();
            endPoint = startPoint;
            aimingTimer = 0f;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, startPoint);
                lineRenderer.enabled = true;
            }

            isAiming = true;
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            aimingTimer += Time.deltaTime;

            if ((limiteModo == LimiteModo.Tempo || limiteModo == LimiteModo.Ambos) && maxDrawTime > 0f)
            {
                if (aimingTimer >= maxDrawTime)
                {
                    if (autoFinalizeOnTimeout)
                    {
                        ValidarCorte();
                        return;
                    }
                    return;
                }
            }

            Vector2 currentMousePos = GetMouseWorldPos();
            endPoint = CalcularEndPointLimitado(currentMousePos);

            if (lineRenderer != null)
                lineRenderer.SetPosition(1, endPoint);
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            ValidarCorte();
        }
    }

    bool IsMouseInsideActionOverlay()
    {
        if (actionOverlay == null) return true;
        if (actionOverlay.rect.width <= 0.01f || actionOverlay.rect.height <= 0.01f) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(actionOverlay, Input.mousePosition, uiCamera);
    }

    Vector2 CalcularEndPointLimitado(Vector2 currentMouseWorldPos)
    {
        Vector2 dir = currentMouseWorldPos - startPoint;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        float distance = Vector2.Distance(startPoint, currentMouseWorldPos);

        if (limiteModo == LimiteModo.Comprimento || limiteModo == LimiteModo.Ambos)
        {
            float maxRadius = (maxRadiusOverride > 0f) ? maxRadiusOverride : maxRadiusFromWeapon;
            distance = Mathf.Min(distance, maxRadius);
        }

        return startPoint + (dir * distance);
    }

    void ValidarCorte()
    {
        isAiming = false;

        float distanciaCortada = Vector2.Distance(startPoint, endPoint);

        float denom;
        if (limiteModo == LimiteModo.Comprimento || limiteModo == LimiteModo.Ambos)
            denom = (maxRadiusOverride > 0f) ? maxRadiusOverride : maxRadiusFromWeapon;
        else
            denom = maxRadiusFromWeapon;

        float precisao = (denom <= 0.0001f) ? 0f : (distanciaCortada / denom);
        bool sucesso = precisao >= minPrecisionToSucceed;

        int weakPointsHit = sucesso ? CountWeakPointsHit_NoCollider(startPoint, endPoint) : 0;

        if (lineRenderer != null) lineRenderer.enabled = false;

        isActive = false;
        gameObject.SetActive(false);

        AttackManager.Instance.FinalizarAtaqueSlashing(sucesso, weakPointsHit, precisao, startPoint, endPoint);
    }

    int CountWeakPointsHit_NoCollider(Vector2 a, Vector2 b)
    {
        WeakPoint[] points = FindObjectsByType<WeakPoint>(FindObjectsSortMode.None);

        int count = 0;
        foreach (var wp in points)
        {
            if (wp == null) continue;

            Vector2 p = wp.transform.position;
            float d = DistancePointToSegment(p, a, b);

            if (d <= slashThickness)
                count++;
        }

        return count;
    }

    static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float abLenSq = ab.sqrMagnitude;

        if (abLenSq < 0.000001f)
            return Vector2.Distance(p, a);

        float t = Vector2.Dot(p - a, ab) / abLenSq;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }

    Vector2 GetMouseWorldPos()
    {
        Vector3 screen = Input.mousePosition;
        Vector3 world = worldCamera.ScreenToWorldPoint(screen);
        world.z = 0f;
        return world;
    }
}