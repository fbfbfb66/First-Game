using UnityEngine;

public abstract class CombatEffectReceiver : MonoBehaviour
{
    public abstract bool CanBeKnockedBack { get; }
    public abstract void SetCanBeKnockedBack(bool value);
    public abstract bool CanBeLaunched { get; }
    public abstract void SetCanBeLaunched(bool value);
    public abstract bool BlocksLocomotion { get; }
    public abstract bool TryApplyKnockback(DisplacementRule rule, float horizontalDirection = 1f);
    public abstract bool TryLaunch(float verticalVelocity, DisplacementRule aiDisplacement = null);
    public abstract bool TryBeginMotionPause();
    public abstract void EndMotionPause();
}
