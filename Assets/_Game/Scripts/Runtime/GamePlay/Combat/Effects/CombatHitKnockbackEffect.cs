using UnityEngine;
[CreateAssetMenu(
    fileName = "Effect_Hit_Knockback",
    menuName = "Game/Combat/Effects/Knockback")]
public class CombatHitKnockbackEffect : CombatHitEffect
{
    public override void Apply(CombatHitEffectContext context)
    {
        if (context.TargetOwner == null) return;
        if (context.WindowDefinition == null) return;

        float facingDirection = context.SourceOwner.transform.position.x > context.TargetOwner.transform.position.x ? -1f : 1f;

        Vector2 temp = context.WindowDefinition.Knockback;
        temp.x = Mathf.Abs(temp.x) * facingDirection;
        Vector2 knockback = temp;

        if (context.TargetOwner.TryGetComponent<CombatEffectReceiver>(out var receiver))
        {
            receiver.TryApplyKnockback(knockback);
        }
    }
}
