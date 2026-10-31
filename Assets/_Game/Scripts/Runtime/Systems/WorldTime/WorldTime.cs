using UnityEngine;

public static class WorldTime
{
    public static float TimeScale { get; private set; } = 1f;
    public static float deltaTime => Time.deltaTime * TimeScale;
    public static float fixedDeltaTime => Time.fixedDeltaTime * TimeScale;

    internal static void SetTimeScale(float timeScale)
    {
        TimeScale = Mathf.Clamp01(timeScale);
    }
}
