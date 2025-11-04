using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Fan Layout Group")]
public class FanLayoutGroup : LayoutGroup
{
    [Tooltip("Total fan angle (degrees)")]
    public float spreadAngle = 40f;
    [Tooltip("Radius / horizontal distance (px)")]
    public float radius = 120f;
    [Tooltip("Vertical offset applied to baseline (relative to parent center). Negative moves baseline down")]
    public float baselineOffset = -60f;
    [Tooltip("Small extra vertical offset proportional to angle magnitude (visual tweak)")]
    public float angleYOffsetFactor = 0.12f;
    [Tooltip("Extra horizontal separation outward (px) to reduce overlap")]
    public float separation = 30f;
    [Tooltip("If true, children's pivot will be forced to bottom (0.5,0) so rotation appears from the bottom edge")]
    public bool useBottomPivot = true;

    public override void CalculateLayoutInputHorizontal() { base.CalculateLayoutInputHorizontal(); }
    public override void CalculateLayoutInputVertical() { }

    public override void SetLayoutHorizontal() { LayoutChildren(); }
    public override void SetLayoutVertical() { LayoutChildren(); }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (radius <= 0f) radius = 120f;
        if (spreadAngle < 0f) spreadAngle = Mathf.Abs(spreadAngle);
        MarkLayoutForRebuild();
    }

    private void LayoutChildren()
    {
        if (!isActiveAndEnabled) return;

        int count = rectChildren.Count;
        if (count == 0) return;

        // Compute baseline (y position where card bottoms should sit).
        float parentHalfHeight = rectTransform.rect.height * 0.5f;
        float baseline = -parentHalfHeight + parentHalfHeight + baselineOffset;

        float half = spreadAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            RectTransform child = rectChildren[i];

            child.anchorMin = child.anchorMax = new Vector2(0.5f, 0.5f);

            if (useBottomPivot)
                child.pivot = new Vector2(0.5f, 0f); 
            else
                child.pivot = new Vector2(0.5f, 0.5f);

            child.localScale = Vector3.one;
        }

        if (count == 1)
        {
            var child = rectChildren[0];
            child.localRotation = Quaternion.identity;
            child.anchoredPosition = new Vector2(0f, baseline);
            child.SetAsLastSibling();
            return;
        }

        // Multiple children -> fan layout
        for (int i = 0; i < count; i++)
        {
            RectTransform child = rectChildren[i];

            float t = (float)i / (count - 1); 

            float angle = Mathf.Lerp(-half, half, t); 
            angle = -angle; 

            child.localRotation = Quaternion.Euler(0f, 0f, angle);

            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad) * radius;

            float sep = Mathf.Lerp(-separation, separation, t);
            x += sep;

            float y = baseline - Mathf.Abs(angle) * angleYOffsetFactor;

            child.anchoredPosition = new Vector2(x, y);
        }
    }

    private void MarkLayoutForRebuild()
    {
        if (!isActiveAndEnabled) return;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
}