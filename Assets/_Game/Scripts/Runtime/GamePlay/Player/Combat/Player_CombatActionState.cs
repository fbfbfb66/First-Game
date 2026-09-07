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

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (TryHandleCancel()) return;


        actionController.TryQueueTransition();
        if (actionController.TryCommitTransition(out var targetAction))
        {
            actionController.BeginAction(targetAction);
            PlayCurrentActionStage();
            return;
        }

        if (actionController.TryCompleteAction())
        {
            ChangeStateToMoveState();
        }
    }

    private void PlayCurrentActionStage()
    {
        stateName = actionController.CurrentAction.Stage.AnimatorStateHash;
        anim.CrossFade(stateName, 0);
        movement.ClearPlayerVelocity();
    }

    private bool TryHandleCancel()
    {
        if (actionController.TryPrepareCancel(out var request) == false) return false;
        if (request.Command == player.DashCommand)
        {
            if (player.TryConsumeDash() == false) return false;
            actionController.CancelCurrentAction();
            stateMachine.ChangeState(player.dashState);
            return true;
        }
        return false;
    }
}
