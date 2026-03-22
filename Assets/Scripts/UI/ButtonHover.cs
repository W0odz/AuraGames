using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Adicione a qualquer GameObject de botão para obter um efeito de escala
/// suave ao passar o mouse por cima. Funciona mesmo com Time.timeScale = 0.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaleAnimation(_originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleAnimation(_originalScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);

        _scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(_rectTransform.localScale, targetScale) > 0.001f)
        {
            _rectTransform.localScale = Vector3.Lerp(
                _rectTransform.localScale,
                targetScale,
                Mathf.Clamp01(Time.unscaledDeltaTime * animationSpeed)
            );
            yield return null;
        }

        _rectTransform.localScale = targetScale;
        _scaleCoroutine = null;
    }
}
