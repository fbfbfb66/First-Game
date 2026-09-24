using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_MotionPause_",
    menuName = "Game/Combat/Effects/Motion Pause")]
public class CombatMotionPauseHitEffect : CombatHitEffect
{
    [SerializeField] private bool pauseSource = true;
    [SerializeField] private bool pauseTarget = true;
    public override void Apply(CombatHitEffectContext context)
    {
        GameObject target = context.TargetOwner;
        GameObject source = context.SourceOwner;
        if (!target || !source) return;
        if (pauseTarget)
        {
            if (target.TryGetComponent<CombatEffectReceiver>(out var targetReceiver))
                targetReceiver.TryBeginMotionPause();
        }

        if (pauseSource)
        {
            if (source.TryGetComponent<CombatEffectReceiver>(out var sourceReceiver))
                sourceReceiver.TryBeginMotionPause();
        }
    }

    public override void End(CombatHitEffectContext context)
    {
        GameObject target = context.TargetOwner;
        GameObject source = context.SourceOwner;
        if (!target || !source) return;
        if (pauseTarget)
        {
            if (target.TryGetComponent<CombatEffectReceiver>(out var targetReceiver))
                targetReceiver.EndMotionPause();
        }

        if (pauseSource)
        {
            if (source.TryGetComponent<CombatEffectReceiver>(out var sourceReceiver))
                sourceReceiver.EndMotionPause();
        }
    }
}
