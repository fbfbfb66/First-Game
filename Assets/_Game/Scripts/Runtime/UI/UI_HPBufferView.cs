using UnityEngine;

public class UI_HPBufferView : UI_HPBarView
{
    [SerializeField, Min(0.01f)] private float duration = 1f;
    [SerializeField] private AnimationCurve speedCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5055542f, 0.009593964f), new Keyframe(1f, 0f));

    private readonly BarFillTransition transition = new BarFillTransition();

    private void Update()
    {
        if (fillClip != null && transition.Tick(Time.deltaTime, duration))
            base.BarEffects(transition.Value);
    }

    protected override void BarEffects(float percent)
    {
        transition.SetTarget(percent, speedCurve,
            !Application.isPlaying || !isActiveAndEnabled || percent >= transition.Value);
        base.BarEffects(transition.Value);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        transition.Reset();
    }
}
