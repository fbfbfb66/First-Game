using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerMovement movement;
    protected PlayerCommandInput commandInput;
    protected GroundSensor groundSensor;
    protected WallSensor wallSensor;
    protected PlayerMotionAnimationTrigger animationTrigger;

    public PlayerState(Player player,StateMachine stateMachine, int stateName, Animator anim) : base(stateMachine, stateName, anim)
    {
        this.player = player;
        movement = player.playerMovement;
        commandInput = player.CommandInput;
        motionController = player.MotionController;
        animationTrigger = player.MotionAnimationTrigger;
        groundSensor = player.groundSensor;
        wallSensor = player.wallSensor;
    }

    protected virtual bool UseAlternateEntryVelocity => false;

    public override void Enter()
    {
        ActionContext context = CreateActionContext();
        stateName = motionController.BeginState(stateName, MotionAction, in context);
        animationTrigger.ResetAnimation();
        base.Enter();
        ApplyEntryVelocity(motionController.CurrentStage?.EntryVelocityRule);
    }

    protected virtual void ApplyEntryVelocity(ActionEntryVelocityRule rule)
    {
        if (rule == null) return;
        movement.SetRigibodyVelocity(rule.Resolve(movement.GetCurrentVelocity(), movement.facingRight,
            commandInput.MoveInput.x, UseAlternateEntryVelocity));
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        groundSensor.UpdateGroundState();
        wallSensor.UpdateWallState(movement.facingRight);

    }

    protected ActionContext CreateActionContext()
    {
        return new ActionContext(
            groundSensor.IsGrounded,
            groundSensor.CanEnterGrounded,
            groundSensor.transform.position,
            player.CanDash,
            player.CanDoubleJump,
            player.Skills,
            commandInput.MoveInput);
    }

    protected void ChangeStateToMoveState()
    {
        if(commandInput.MoveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if(isSameDirctionForWallandFacingDir() == false)
        {
            if(movement.playerMoveType == PlayerMoveType.Run)
            {
                stateMachine.ChangeState(player.runState);
            }
            else if(movement.playerMoveType == PlayerMoveType.Walk)
            {
                stateMachine.ChangeState(player.walkState);
            }
        }
        else
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override bool CanTransitionTo(EntityState targetState, StateTransitionKind transitionKind)
    {
        if (transitionKind == StateTransitionKind.Cancel)
            return targetState == player.dashState && player.DashCommand != null &&
                motionController.CanInterrupt(player.DashCommand) &&
                player.CombatActionController.CanInterrupt(player.DashCommand);
        return base.CanTransitionTo(targetState, transitionKind);
    }

    protected bool TryCancelToDash()
    {
        if (player.DashCommand == null ||
            !player.MotionController.TryGetValidBufferedRequest(out var request) ||
            request.Command != player.DashCommand) return false;
        if (!stateMachine.CanChangeState(player.dashState, StateTransitionKind.Cancel)) return false;
        ActionContext context = CreateActionContext();
        if (!motionController.TryStartAction(player.DashAction, in context)) return false;
        if (!player.CombatActionController.TryInterrupt(player.DashCommand) ||
            !motionController.TryInterrupt(player.DashCommand)) return false;
        return stateMachine.TryChangeState(player.dashState, StateTransitionKind.Cancel);
    }

    protected bool TryStartBackDodge()
    {
        if (player.BackDodgeCommand == null || player.MotionController.BufferedRequest.Command != player.BackDodgeCommand)
            return false;

        ActionContext context = CreateActionContext();
        if (!motionController.TryStartAction(player.BackDodgeAction, in context)) return false;
        return stateMachine.TryChangeState(player.backDodgeState, StateTransitionKind.Natural);
    }

    protected bool isSameDirctionForWallandFacingDir(bool needToHandleKey = true)
    {
        if (wallSensor.IsTouchingWall == false) return false;
        Vector2 move = commandInput.MoveInput;
        bool isSameDir = false;
        if (move.x > 0 && movement.facingRight)
        {
            isSameDir = true;
        }
        if (move.x < 0 && movement.facingRight == false)
        {
            isSameDir = true;
        }
        if (needToHandleKey == false && move.x == 0) return true;
        return isSameDir;
    }

    public override void Exit()
    {
        motionController.EndState(stateName);
        base.Exit();
    }
}
