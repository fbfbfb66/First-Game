using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_Launch_",
    menuName = "Game/Actions/Combat/Effects/Launch")]
public sealed class CombatLaunchHitEffect : CombatHitEffect
{
    [SerializeField] private float targetVerticalVelocity;
    [SerializeField] private bool launchSourceOnSuccess = true;
    [SerializeField] private float sourceVerticalVelocity;

    [SerializeField] private DisplacementRule aiTargetDisplacement = new();
    [SerializeField] private DisplacementRule aiSourceDisplacement = new();

    public override void Apply(CombatHitEffectContext context)
    {
        GameObject target = context.TargetOwner;
        GameObject  source = context.SourceOwner;
        if (!target || !source) return;
        if (target.TryGetComponent<CombatEffectReceiver>(out var receiver) == false) return;
        if (receiver.TryLaunch(targetVerticalVelocity, aiTargetDisplacement) == false) return;
        if (launchSourceOnSuccess == false) return;
        if (source.TryGetComponent<CombatEffectReceiver>(out var sourceReceiver))
            sourceReceiver.TryLaunch(sourceVerticalVelocity, aiSourceDisplacement);
    }
}
