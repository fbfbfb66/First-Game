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
        if (!CanChangeState(stateChangeTo))
        {
            return;
        }
        currentState.Exit();
        currentState = stateChangeTo;
        currentState.Enter();
    }

    public void LogicalUpdate()
    {
        currentState.LogicalUpdate();
    }

    public void PhysicalUpdate()
    {
        currentState.PhysicalUpdate();
    }

    private bool CanChangeState(EntityState stateChangeTo)
    {
        return stateChangeTo != currentState;
    }
}
