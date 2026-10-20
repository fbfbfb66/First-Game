using UnityEngine;
using UnityEngine.UI;

public class UI_EnergyBarView : UI_ResourceBarView
{
    private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
    [SerializeField, Min(0.01f)] private float duration = 1f;
    [SerializeField] private AnimationCurve speedCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5055542f, 0.009593964f), new Keyframe(1f, 0f));

    private readonly BarFillTransition transition = new BarFillTransition();
    protected virtual bool AnimateFill => true;

#if UNITY_EDITOR
    [ContextMenu("Debug/Consume 10% Energy")]
    private void DebugConsumeEnergy()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning($"{name} Energy debug requires Play Mode. Fill unchanged.", this);
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
            ApplyFill(transition.Value);
    }

    protected override void BarEffects(float percent)
    {
        transition.SetTarget(percent, speedCurve,
            !AnimateFill || !Application.isPlaying || !isActiveAndEnabled);
        ApplyFill(transition.Value);
    }

    private void ApplyFill(float percent)
    {
        if (!fillClip.TryGetComponent<Graphic>(out var graphic)) return;
        Material material = graphic.material;
        if (material == null || !material.HasProperty(FillAmountId))
        {
            Debug.LogWarning("Energy Fill needs a material with a _FillAmount property.", this);
            return;
        }

        material.SetFloat(FillAmountId, percent);
        graphic.materialForRendering.SetFloat(FillAmountId, percent);
    }
}
