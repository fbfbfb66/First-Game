using UnityEngine;
using UnityEngine.Serialization;

public class UI_ResourceBarView : MonoBehaviour
{
    [SerializeField] protected RectTransform fillClip;
    [SerializeField, Min(0f)] protected float fullWidth = 155f;
    [FormerlySerializedAs("fill")]
    [SerializeField, Range(0f, 1f)] protected float fillPercent = 1f;

    public virtual void SetPercent(float percent)
    {
        if (fillClip == null) return;
        if (percent < 0f || percent > 1f)
        {
            Debug.LogWarning("Percent must be between 0 and 1; the resource bar was not updated.", this);
            return;
        }

        fillPercent = percent;
        BarEffects(percent);
    }

    protected virtual void BarEffects(float percent)
    {
        fillClip.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth * percent);
    }

#if UNITY_EDITOR
    protected void OnValidate()
    {
        SetPercent(fillPercent);
    }
#endif
}
