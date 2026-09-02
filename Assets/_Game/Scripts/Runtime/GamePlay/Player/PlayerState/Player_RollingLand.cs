using UnityEngine;

public class Player_RollingLand : PlayerState
{
    private float rollingLandSpeed;
    public Player_RollingLand(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.ClearYVelocity();
        animationTrigger.StartAnimation();
        rollingLandSpeed = Mathf.Abs(movement.Rb.linearVelocity.x) <= player.playerBaseConfig.RollingLandDefaultVelocity ? player.playerBaseConfig.RollingLandDefaultVelocity : Mathf.Abs(movement.Rb.linearVelocity.x);
        rollingLandSpeed = movement.facingRight ? rollingLandSpeed : -rollingLandSpeed;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(groundSensor.IsGrounded && animationTrigger.IsAnimationFinished)
        {
            ChangeStateToMoveState();
        }
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        movement.SetRigibodyVelocity(new Vector2(rollingLandSpeed, 0));
    }
}
