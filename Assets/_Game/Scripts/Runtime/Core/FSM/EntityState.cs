using UnityEngine;

public class EntityState 
{
    protected StateMachine stateMachine;
    protected Animator anim;
    protected int stateName;
    public int AnimatorStateHash => stateName;
    public EntityState(StateMachine stateMachine,int stateName,Animator anim)
    {
        this.stateMachine = stateMachine;
        this.stateName = stateName;
        this.anim = anim;
    }

    public virtual void Enter()
    {
        anim.CrossFade(stateName,0);
    }

    public virtual void LogicalUpdate()
    {
        
    }

    public virtual void PhysicalUpdate()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual bool CanTransitionTo(EntityState targetState,StateTransitionKind transitionKind)
    {
        if (StateTransitionKind.Cancel == transitionKind) return false;
        return true;
    }
}
