using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target;

    [Header("Limites do Mapa")]
    [Tooltip("Collider2D que define as bordas da sala (mesmo usado pelo EnemyAIController no campo mapBoundsCollider)")]
    public Collider2D mapBoundsCollider;

    [Header("Suavização (opcional)")]
    [Tooltip("0 = sem suavização, valores maiores = mais suave. Recomendado: 5 a 8")]
    public float smoothSpeed = 0f;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desejada = new Vector3(target.position.x, target.position.y, -10f);

        // Aplica suavização se configurado
        Vector3 posicao = smoothSpeed > 0f
            ? Vector3.Lerp(transform.position, desejada, smoothSpeed * Time.deltaTime)
            : desejada;

        // Aplica clamp nos limites do mapa se tiver collider configurado
        if (mapBoundsCollider != null && _cam != null)
        {
            float camHalfHeight = _cam.orthographicSize;
            float camHalfWidth  = camHalfHeight * _cam.aspect;

            Bounds b = mapBoundsCollider.bounds;

            float minX = b.min.x + camHalfWidth;
            float maxX = b.max.x - camHalfWidth;
            posicao.x = minX <= maxX ? Mathf.Clamp(posicao.x, minX, maxX) : b.center.x;

            float minY = b.min.y + camHalfHeight;
            float maxY = b.max.y - camHalfHeight;
            posicao.y = minY <= maxY ? Mathf.Clamp(posicao.y, minY, maxY) : b.center.y;
        }

        transform.position = posicao;
    }
}