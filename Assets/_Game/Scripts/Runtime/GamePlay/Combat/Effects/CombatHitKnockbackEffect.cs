using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_Hit_Knockback",
    menuName = "Game/Actions/Combat/Effects/Knockback")]
public class CombatHitKnockbackEffect : CombatHitEffect
{
    public override void Apply(CombatHitEffectContext context)
    {
        if (context.TargetOwner == null || context.SourceOwner == null) return;
        if (context.WindowDefinition == null) return;

        float facingDirection = context.SourceOwner.transform.position.x > context.TargetOwner.transform.position.x ? -1f : 1f;

        if (context.TargetOwner.TryGetComponent<CombatEffectReceiver>(out var receiver))
        {
            receiver.TryApplyKnockback(context.WindowDefinition.Knockback, facingDirection);
        }
    }
}
