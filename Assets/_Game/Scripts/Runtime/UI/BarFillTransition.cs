using UnityEngine;

/// <summary>Integrates a relative speed curve to animate a fill percentage.</summary>
public sealed class BarFillTransition
{
    private const int Samples = 64;
    private readonly float[] cumulativeSpeed = new float[Samples + 1];
    private float start;
    private float target;
    private float elapsed;
    private bool initialized;
    private bool animating;

    public float Value { get; private set; }

    public void SetTarget(float percent, AnimationCurve speedCurve, bool immediate)
    {
        if (!initialized || immediate)
        {
            Value = target = percent;
            initialized = true;
            animating = false;
            return;
        }
        if (percent == target) return;

        start = Value;
        target = percent;
        elapsed = 0f;
        cumulativeSpeed[0] = 0f;
        float previous = speedCurve == null ? 1f : Mathf.Max(0f, speedCurve.Evaluate(0f));
        for (int i = 1; i <= Samples; i++)
        {
            float speed = speedCurve == null ? 1f : Mathf.Max(0f, speedCurve.Evaluate((float)i / Samples));
            cumulativeSpeed[i] = cumulativeSpeed[i - 1] + (previous + speed) * 0.5f / Samples;
            previous = speed;
        }
        animating = true;
    }

    public bool Tick(float deltaTime, float duration)
    {
        if (!animating) return false;

        elapsed += deltaTime;
        float time = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
        float sample = time * Samples;
        int index = Mathf.Min(Mathf.FloorToInt(sample), Samples - 1);
        float area = Mathf.Lerp(cumulativeSpeed[index], cumulativeSpeed[index + 1], sample - index);
        float total = cumulativeSpeed[Samples];
        Value = Mathf.Lerp(start, target, total > Mathf.Epsilon ? area / total : time);
        if (time >= 1f)
        {
            Value = target;
            animating = false;
        }
        return true;
    }

    public void Reset()
    {
        initialized = false;
        animating = false;
    }
}
