using UnityEngine;

public abstract class CombatHitEffect : ScriptableObject
{
    public abstract void Apply(CombatHitEffectContext context);

    public virtual void End(CombatHitEffectContext context)
    {
    }
}
