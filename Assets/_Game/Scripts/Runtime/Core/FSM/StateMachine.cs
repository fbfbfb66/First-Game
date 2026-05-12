public class StateMachine 
{
    private EntityState currentState;

    public void InitializeState(EntityState currentState)
    {
        if(this.currentState == null)
            this.currentState = currentState;
    }

    public void ChangeState(EntityState stateChangeTo)
    {
        currentState.Exit();
        currentState = stateChangeTo;
        currentState.Enter();
    }

    public void LogicialUpdate()
    {
        currentState.LogicalUpdate();
    }

    public void PhysicalUpdate()
    {
        currentState.PhysicalUpdate();
    }
}
