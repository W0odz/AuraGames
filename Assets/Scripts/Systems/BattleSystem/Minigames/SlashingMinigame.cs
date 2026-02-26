using UnityEngine;
using UnityEngine.UI;

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

    [Header("Timer (UI)")]
    public Slider timeSlider;

    [Header("Detecção (weakpoints sem Collider2D)")]
    public float slashThickness = 0.35f;

    [Header("Regra de sucesso")]
    [Range(0f, 1f)]
    public float minPrecisionToSucceed = 0.5f;

    [Header("Limite do desenho")]
    public LimiteModo limiteModo = LimiteModo.Ambos;
    public float maxRadiusOverride = 0f;
    public float maxDrawTime = 5f;
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

    bool UsaTempo()
    {
        return (limiteModo == LimiteModo.Tempo || limiteModo == LimiteModo.Ambos) && maxDrawTime > 0f;
    }

    void SetTimerVisible(bool visible)
    {
        if (timeSlider != null)
            timeSlider.gameObject.SetActive(visible);
    }

    void SetTimeSliderValue01(float v)
    {
        if (timeSlider == null) return;

        v = Mathf.Clamp01(v);
        timeSlider.minValue = 0f;
        timeSlider.maxValue = 1f;
        timeSlider.value = v;

        // some com o Fill quando chegar a 0
        if (timeSlider.fillRect != null)
            timeSlider.fillRect.gameObject.SetActive(v > 0.0001f);
    }

    void UpdateTimerUI()
    {
        if (timeSlider == null) return;
        if (maxDrawTime <= 0f) return;

        SetTimeSliderValue01(1f - Mathf.Clamp01(aimingTimer / maxDrawTime));
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

        // IMPORTANTE: timer começa aqui (quando o minigame inicia)
        aimingTimer = 0f;

        isAiming = false;
        isActive = true;

        bool usaTempo = UsaTempo();
        SetTimerVisible(usaTempo);
        SetTimeSliderValue01(1f);

        gameObject.SetActive(true);
    }

    // Permite iniciar o corte no mesmo clique que iniciou o ataque (sem precisar clicar 2x)
    // Não reseta timer: ele já está correndo desde o Iniciar()
    public void BeginSlashFromWorldPoint(Vector2 worldStart)
    {
        if (!isActive) return;
        if (worldCamera == null) worldCamera = Camera.main;

        startPoint = worldStart;
        endPoint = startPoint;

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, startPoint);
            lineRenderer.enabled = true;
        }

        isAiming = true;
    }

    void Update()
    {
        if (!isActive) return;
        if (worldCamera == null) return;

        bool usaTempo = UsaTempo();

        // Timer sempre corre a partir do início do minigame
        if (usaTempo)
        {
            aimingTimer += Time.deltaTime;
            UpdateTimerUI();

            if (aimingTimer >= maxDrawTime)
            {
                if (autoFinalizeOnTimeout)
                {
                    ValidarCorte();
                    return;
                }
                else
                {
                    // trava o input se você quiser (aqui só impede de avançar)
                    return;
                }
            }
        }

        // Fallback: iniciar mirando por mouse down (caso você não use BeginSlashFromWorldPoint)
        if (!isAiming && Input.GetMouseButtonDown(0))
        {
            if (!IsMouseInsideActionOverlay()) return;

            startPoint = GetMouseWorldPos();
            endPoint = startPoint;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, startPoint);
                lineRenderer.enabled = true;
            }

            isAiming = true;
        }

        // Atualizar linha enquanto segura
        if (isAiming && Input.GetMouseButton(0))
        {
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
        SetTimerVisible(false);
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