using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LetterButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [Tooltip("Target scale when hovered.")]
    public float hoveredScale = 1.12f;
    [Tooltip("Animation duration in seconds.")]
    public float duration = 0.12f;
    [Tooltip("Smoothness curve for scaling animation.")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lift Effect")]
    [Tooltip("How much the element moves upward on hover (UI units / px). Avoid setting too high.")]
    public float hoverOffsetY = 8f;

    [Header("Border Effect")]
    [Tooltip("RectTransform used for the hover border (usually a child object with an Image).")]
    public RectTransform borderRect;
    [Tooltip("Image component used to control the border’s color and alpha animation.")]
    public Image borderImage;
    [Tooltip("Extra scale factor for the border to make it slightly larger than the main element.")]
    public float borderScaleExtra = 1.06f;
    [Tooltip("Border color (default is a soft red tone).")]
    public Color borderColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Anti-Jitter (debounce)")]
    [Tooltip("Delay (seconds) before performing unhover after pointer exit. Increase to reduce jitter.")]
    public float unhoverDelay = 0.08f;

    RectTransform rt;
    Coroutine current;

    bool pointerOver = false;   
    bool isPressed = false;     

    Vector2 baseAnchoredPosition;
    bool baseRecorded = false;

    Coroutine unhoverCoroutine;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        if (borderImage != null)
        {
            borderImage.raycastTarget = false;
        }
        if (borderRect != null)
        {
            var img = borderRect.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
        }

        if (borderRect != null)
            borderRect.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unhoverCoroutine != null)
        {
            StopCoroutine(unhoverCoroutine);
            unhoverCoroutine = null;
        }

        pointerOver = true;
        TryRecordBasePosition();
        StartSmoothScale(hoveredScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (unhoverCoroutine != null)
            StopCoroutine(unhoverCoroutine);
        unhoverCoroutine = StartCoroutine(DelayedUnhover());
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (unhoverCoroutine != null) { StopCoroutine(unhoverCoroutine); unhoverCoroutine = null; }
        pointerOver = true;
        TryRecordBasePosition();
        StartSmoothScale(hoveredScale);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (unhoverCoroutine != null) { StopCoroutine(unhoverCoroutine); unhoverCoroutine = null; }
        pointerOver = false;
        if (!isPressed)
            StartSmoothScale(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        if (current != null) StopCoroutine(current);
        rt.localScale = Vector3.one * hoveredScale;
        if (!baseRecorded) TryRecordBasePosition();
        rt.anchoredPosition = baseAnchoredPosition + Vector2.up * hoverOffsetY;

        if (borderRect != null && borderImage != null)
        {
            borderRect.gameObject.SetActive(true);
            borderRect.localScale = Vector3.one * borderScaleExtra * hoveredScale;
            borderImage.color = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        if (pointerOver)
            StartSmoothScale(hoveredScale);
        else
            StartSmoothScale(1f);
    }

    IEnumerator DelayedUnhover()
    {
        yield return new WaitForSecondsRealtime(unhoverDelay);

        pointerOver = false;
        if (!isPressed)
            StartSmoothScale(1f);

        unhoverCoroutine = null;
    }

    void TryRecordBasePosition()
    {
        RectTransform parentRt = transform.parent as RectTransform;
        if (parentRt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
        }

        baseAnchoredPosition = rt.anchoredPosition;
        baseRecorded = true;
    }

    void StartSmoothScale(float target)
    {
        if (!baseRecorded)
        {
            TryRecordBasePosition();
        }

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(SmoothScaleRoutine(target));
    }

    IEnumerator SmoothScaleRoutine(float target)
    {
        Vector3 startScale = rt.localScale;
        Vector3 endScale = Vector3.one * target;

        Vector2 startAnch = rt.anchoredPosition;
        Vector2 endAnch = (target > 1f) ? baseAnchoredPosition + Vector2.up * hoverOffsetY : baseAnchoredPosition;

        bool useBorder = (borderRect != null && borderImage != null);
        Vector3 borderStartScale = (useBorder ? borderRect.localScale : Vector3.one);
        Vector3 borderEndScale = Vector3.one * borderScaleExtra * target;
        Color borderStartColor = new Color(borderColor.r, borderColor.g, borderColor.b, 0f);
        Color borderEndColor = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a);

        if (useBorder)
        {
            if (!borderRect.gameObject.activeSelf) borderRect.gameObject.SetActive(true);
            borderImage.color = borderStartColor;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / duration);
            float k = curve.Evaluate(f);

            rt.localScale = Vector3.LerpUnclamped(startScale, endScale, k);
            rt.anchoredPosition = Vector2.Lerp(startAnch, endAnch, k);

            if (useBorder)
            {
                borderRect.localScale = Vector3.Lerp(borderStartScale, borderEndScale, k);
                borderImage.color = Color.Lerp(borderStartColor, borderEndColor, k);
            }

            yield return null;
        }

        rt.localScale = endScale;
        rt.anchoredPosition = endAnch;

        if (useBorder)
        {
            borderRect.localScale = borderEndScale;
            borderImage.color = borderEndColor;

            if (!pointerOver && !isPressed)
            {
                borderRect.gameObject.SetActive(false);
            }
        }

        if (!pointerOver && !isPressed)
            baseRecorded = false;

        current = null;
    }
}
