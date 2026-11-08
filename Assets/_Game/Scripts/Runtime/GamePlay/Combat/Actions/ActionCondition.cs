using UnityEngine;

public abstract class ActionCondition : ScriptableObject
{
    // A condition requiring combat-only facts cannot be satisfied by general facts alone.
    public virtual bool IsMet(in ActionContext context) => false;

    public virtual bool IsMet(in AIActionContext context) => false;

    public virtual bool IsMet(in CombatActionContext context)
    {
        ActionContext actionContext = new ActionContext(context.IsGrounded, context.CanEnterGrounded, context.GroundProbePosition);
        return IsMet(in actionContext);
    }
}
