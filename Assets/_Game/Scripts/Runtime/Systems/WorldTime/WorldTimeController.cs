using UnityEngine;

public class WorldTimeController : MonoBehaviour
{
    public static WorldTimeController Instance { get; private set; }

    [Tooltip("X: recovery progress (0–1). Y: recovery amount (0–1).")]
    [SerializeField] private AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public AnimationCurve RecoveryCurve => recoveryCurve;

    private float startScale;
    private float totalDuration;
    private float remainingDuration;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (remainingDuration <= 0f)
            return;

        remainingDuration -= Time.unscaledDeltaTime;
        if (remainingDuration <= 0f)
        {
            ResetWorldTime();
            return;
        }

        float progress = Mathf.Clamp01(1f - remainingDuration / totalDuration);
        float recoveryAmount = recoveryCurve != null && recoveryCurve.length > 0
            ? recoveryCurve.Evaluate(progress)
            : progress;
        WorldTime.SetTimeScale(Mathf.Lerp(startScale, 1f, recoveryAmount));
    }
    private void OnDestroy()
    {
        if (Instance != this)
            return;

        ResetWorldTime();
        Instance = null;
    }

    public void RequestSlowMotion(float targetScale, float duration)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning("Duration must be greater than zero.");
            return;
        }

        WorldTime.SetTimeScale(targetScale);
        startScale = WorldTime.TimeScale;
        totalDuration = duration;
        remainingDuration = duration;
    }

    public void ResetWorldTime()
    {
        remainingDuration = 0f;
        totalDuration = 0f;
        startScale = 1f;
        WorldTime.SetTimeScale(1f);
        Debug.Log("World time reset to normal.");
    }

    [ContextMenu("Test Slow Motion")]
    private void TestSlowMotion()
    {
        RequestSlowMotion(0.2f, 4f);
    }
}
