public class StateMachine 
{
    public EntityState currentState{ get; private set; }

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
