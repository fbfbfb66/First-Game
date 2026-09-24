using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_Launch_",
    menuName = "Game/Combat/Effects/Launch")]
public sealed class CombatLaunchHitEffect : CombatHitEffect
{
    [SerializeField] private float targetVerticalVelocity;
    [SerializeField] private bool launchSourceOnSuccess = true;
    [SerializeField] private float sourceVerticalVelocity;

    public override void Apply(CombatHitEffectContext context)
    {
        GameObject target = context.TargetOwner;
        GameObject  source = context.SourceOwner;
        if (!target || !source) return;
        if (target.TryGetComponent<CombatEffectReceiver>(out var receiver) == false) return;
        if (receiver.TryLaunch(targetVerticalVelocity) == false) return;
        if (launchSourceOnSuccess == false) return;
        Rigidbody2D body = source.GetComponent<Rigidbody2D>();
        Vector2 temp = body.linearVelocity;
        temp.y = sourceVerticalVelocity;
        body.linearVelocity = temp;
    }
}
