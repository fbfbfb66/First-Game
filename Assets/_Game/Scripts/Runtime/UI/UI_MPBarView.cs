using UnityEngine;

public sealed class UI_MPBarView : UI_ResourceBarView
{
    [SerializeField, Min(0.01f)] private float duration = 1f;
    [SerializeField] private AnimationCurve speedCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5055542f, 0.009593964f), new Keyframe(1f, 0f));

    private readonly BarFillTransition transition = new BarFillTransition();

#if UNITY_EDITOR
    [ContextMenu("Debug/Consume 10% MP")]
    private void DebugConsumeMP()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning($"{name} MP debug requires Play Mode. Fill unchanged.", this);
            return;
        }

        SetPercent(Mathf.Max(0f, fillPercent - 0.1f));
    }
#endif

    private void OnEnable()
    {
        SetPercent(fillPercent);
    }

    private void OnDisable()
    {
        transition.Reset();
    }

    private void Update()
    {
        if (fillClip != null && transition.Tick(Time.deltaTime, duration))
            base.BarEffects(transition.Value);
    }

    protected override void BarEffects(float percent)
    {
        transition.SetTarget(percent, speedCurve, !Application.isPlaying || !isActiveAndEnabled);
        base.BarEffects(transition.Value);
    }

    public override void SetPercent(float percent)
    {
        base.SetPercent(percent);
    }
}
