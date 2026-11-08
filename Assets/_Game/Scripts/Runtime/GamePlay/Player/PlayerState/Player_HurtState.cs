using UnityEngine;

public sealed class Player_HurtState : PlayerState
{
    protected override MotionActionDefinition MotionAction => player.HurtAction;

    public Player_HurtState(Player player, StateMachine stateMachine, Animator anim)
        : base(player, stateMachine, 0, anim) { }

    private float hitDirection;

    public override void Enter()
    {
        base.Enter();
        Vector2 dir = new Vector2(-hitDirection, 0f);
        movement.HandleFlip(dir);
    }

    public void SetHitSourcePosition(Vector2 sourcePosition)
    {
        float offset = player.transform.position.x - sourcePosition.x;
        hitDirection = Mathf.Approximately(offset, 0f)
            ? (movement.facingRight ? -1f : 1f)
            : Mathf.Sign(offset);
    }

    public override bool CanTransitionTo(EntityState targetState, StateTransitionKind transitionKind)
    {
        if (transitionKind == StateTransitionKind.Cancel)
            return base.CanTransitionTo(targetState, transitionKind);
        return animationTrigger.IsAnimationFinished && base.CanTransitionTo(targetState, transitionKind);
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (TryCancelToDash()) return;
        if (!animationTrigger.IsAnimationFinished) return;
        if (groundSensor.CanEnterGrounded)
        {
            // Idle can safely choose movement on its following update, even against a wall.
            stateMachine.ChangeState(player.idleState);
            player.RestoreAirborneResources();
            return;
        }
        float yVelocity = movement.GetCurrentVelocity().y;
        float threshold = player.playerBaseConfig.ApexThreshold;
        if (yVelocity > threshold) stateMachine.ChangeState(player.jumpUpState);
        else if (yVelocity < -threshold) stateMachine.ChangeState(player.fallState);
        else stateMachine.ChangeState(player.apexState);
    }
}
