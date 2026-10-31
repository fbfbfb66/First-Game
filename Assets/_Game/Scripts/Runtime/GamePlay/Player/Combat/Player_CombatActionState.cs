using UnityEngine;

public sealed class Player_CombatActionState : PlayerState
{
    private readonly PlayerCombatActionController actionController;
    public Player_CombatActionState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
        actionController = player.CombatActionController;
    }

    public override void Enter()
    {
        PlayCurrentActionStage();
    }

    public override void Exit()
    {
        actionController.CancelCurrentAction();
        base.Exit();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (TryHandleCancel()) return;

        CombatActionContext context = player.CreateCombatActionContext();
        actionController.TryQueueTransition(in context);
        actionController.TryQueueAutomaticTransition(in context);
        if (actionController.TryCommitTransition(out var targetAction))
        {
            actionController.BeginAction(targetAction);
            PlayCurrentActionStage();
            return;
        }

        if (actionController.TryCompleteAction())
        {
            ReturnToLocomotionState();
        }
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        ApplyHorizontalDeceleration(actionController.CurrentCombatStage.HorizontalDeceleration);
    }

    private void PlayCurrentActionStage()
    {
        stateName = actionController.CurrentStage.AnimatorStateHash;
        anim.CrossFade(stateName, 0);
        ApplyEntryVelocity(actionController.CurrentCombatStage.EntryVelocityRule);
    }

    private void ApplyHorizontalDeceleration(float deceleration)
    {
        if (deceleration <= 0) return;
        Vector2 velocity = movement.GetCurrentVelocity();
        velocity.x = Mathf.MoveTowards(velocity.x, 0,deceleration*Time.fixedDeltaTime);
        movement.SetRigibodyVelocity(velocity);
    }

    private void ApplyEntryVelocity(CombatActionEntryVelocityRule rule)
    {
        if (rule == null) return;
        Vector2 entryVelocity = rule.Resolve(movement.GetCurrentVelocity(),movement.facingRight);
        movement.SetRigibodyVelocity(entryVelocity);
    }

    private bool TryHandleCancel()
    {
        if (!motionController.TryGetValidBufferedRequest(out var request)) return false;
        if (!actionController.AllowsCancel(request.Command)) return false;
        if (request.Command == player.DashCommand)
        {
            ActionContext context = CreateActionContext();
            if (!motionController.TryStartAction(player.DashAction, in context)) return false;
            stateMachine.ChangeState(player.dashState);
            return true;
        }
        return false;
    }

    private void ReturnToLocomotionState()
    {
        if (groundSensor.CanEnterGrounded)
        {
            ChangeStateToMoveState();
            player.RestoreAirborneResources();
        }
        else
        {
            float yVelocity = movement.GetCurrentVelocity().y;
            float apexThreshold = player.playerBaseConfig.ApexThreshold;
            if (yVelocity > apexThreshold)
            {
                stateMachine.ChangeState(player.jumpUpState);
                return;
            }
            if (yVelocity <= -apexThreshold)
            {
                stateMachine.ChangeState(player.fallState);
                return;
            }
            stateMachine.ChangeState(player.apexState);
        }
    }
}
