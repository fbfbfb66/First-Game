using UnityEngine;

public abstract class MotionStageEffect : ScriptableObject
{
    public abstract MotionStageEffectInstance CreateInstance(GameObject owner);
}

public abstract class MotionStageEffectInstance
{
    public abstract void Begin();
    public abstract void End();
}
