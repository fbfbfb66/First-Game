using UnityEngine;

[RequireComponent(typeof(AIDisplacementMotor))]
public sealed class AICombatEffectReceiver : CombatEffectReceiver
{
    [SerializeField] private AIDisplacementMotor motor;
    [SerializeField] private bool canBeKnockedBack = true;
    [SerializeField] private bool canBeLaunched = true;
    private bool isMotionPaused;
    public override bool CanBeKnockedBack => canBeKnockedBack;
    public override void SetCanBeKnockedBack(bool value) => canBeKnockedBack = value;
    public override bool CanBeLaunched => canBeLaunched;
    public override void SetCanBeLaunched(bool value) => canBeLaunched = value;
    public override bool BlocksLocomotion => isMotionPaused || (motor != null && motor.IsDisplacementActive);

    private void Awake()
    {
        if (motor == null) motor = GetComponent<AIDisplacementMotor>();
    }

    public override bool TryApplyKnockback(DisplacementRule rule, float horizontalDirection = 1f)
    {
        return isActiveAndEnabled && canBeKnockedBack && motor != null &&
            motor.TryStartDisplacement(rule, horizontalDirection, this, true);
    }

    public override bool TryLaunch(float verticalVelocity, DisplacementRule aiDisplacement = null)
    {
        return isActiveAndEnabled && canBeLaunched && motor != null &&
            motor.TryStartDisplacement(aiDisplacement, motor.FacingDirection, this, true);
    }

    public override bool TryBeginMotionPause()
    {
        if (!isActiveAndEnabled || motor == null || isMotionPaused) return false;
        isMotionPaused = true;
        motor.SetPaused(true);
        return true;
    }

    public override void EndMotionPause()
    {
        if (!isMotionPaused) return;
        isMotionPaused = false;
        if (motor != null) motor.SetPaused(false);
    }

    private void OnDisable()
    {
        if (motor != null) motor.StopDisplacement(this);
        EndMotionPause();
    }
}
