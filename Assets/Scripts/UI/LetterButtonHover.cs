using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class LetterButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float hoveredScale = 1.12f;
    public float duration = 0.12f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    RectTransform rt;
    Coroutine current;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartSmoothScale(hoveredScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartSmoothScale(1f);
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartSmoothScale(hoveredScale);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StartSmoothScale(1f);
    }

    void StartSmoothScale(float target)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(SmoothScaleRoutine(target));
    }

    IEnumerator SmoothScaleRoutine(float target)
    {
        Vector3 start = rt.localScale;
        Vector3 end = Vector3.one * target;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / duration);
            float k = curve.Evaluate(f);
            rt.localScale = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }
        rt.localScale = end;
        current = null;
    }
}