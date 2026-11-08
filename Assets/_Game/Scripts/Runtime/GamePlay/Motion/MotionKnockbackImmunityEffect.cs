using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Motion_KnockbackImmunity", menuName = "Game/Actions/Motion/Effects/Knockback Immunity")]
public sealed class MotionKnockbackImmunityEffect : MotionStageEffect
{
    public override MotionStageEffectInstance CreateInstance(GameObject owner)
    {
        if (owner != null && owner.TryGetComponent<CombatEffectReceiver>(out var receiver)) return new Instance(receiver);
        Debug.LogWarning($"[MotionEffect] {name}: owner has no CombatEffectReceiver; knockback immunity not applied.", owner);
        return null;
    }

    private sealed class Instance : MotionStageEffectInstance
    {
        private readonly CombatEffectReceiver receiver;
        private bool previousKnockback;
        private bool previousLaunch;
        public Instance(CombatEffectReceiver receiver) => this.receiver = receiver;
        public override void Begin()
        {
            previousKnockback = receiver.CanBeKnockedBack;
            previousLaunch = receiver.CanBeLaunched;
            receiver.SetCanBeKnockedBack(false);
            receiver.SetCanBeLaunched(false);
        }
        public override void End()
        {
            if (receiver == null) return;
            receiver.SetCanBeKnockedBack(previousKnockback);
            receiver.SetCanBeLaunched(previousLaunch);
        }
    }
}
