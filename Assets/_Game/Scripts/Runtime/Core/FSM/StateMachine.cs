using UnityEngine;
public class StateMachine
{
    public EntityState currentState{ get; private set; }

    public void InitializeState(EntityState currentState)
    {
        if(this.currentState == null)
        {
            this.currentState = currentState;
            currentState.Enter();
        }
    }

    public void ChangeState(EntityState stateChangeTo)
    {
        TryChangeState(stateChangeTo, StateTransitionKind.Natural);
    }

    public bool CanChangeState(EntityState targetState,StateTransitionKind transitionKind)
    {
        if (targetState == null)
        {
            Debug.LogWarning("targetState is null");
            return false;
        }
        if (currentState == null)
        {
            Debug.LogWarning("CurrentState is null");
            return false;
        }
        if (currentState == targetState) return false;
        if (transitionKind == StateTransitionKind.Forced) return true;
        return currentState.CanTransitionTo(targetState, transitionKind);
    }

    public bool TryChangeState(EntityState targetState, StateTransitionKind transitionKind)
    {
        if (CanChangeState(targetState, transitionKind) == false) return false;

        CommitStateChange(targetState);
        return true;
    }

    public void LogicalUpdate()
    {
        currentState.LogicalUpdate();
    }

    public void PhysicalUpdate()
    {
        currentState.PhysicalUpdate();
    }

    private void CommitStateChange(EntityState targetState)
    {
        currentState.Exit();
        currentState = targetState;
        currentState.Enter();
    }
}
