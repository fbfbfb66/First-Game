
public readonly struct CombatActionContext
{
    public bool IsGrounded { get; }
    private readonly CombatActionChainRuntime chainRuntime;
    public CombatActionContext(bool isGrounded,CombatActionChainRuntime chainRuntime)
    {
        this.IsGrounded = isGrounded;
        this.chainRuntime = chainRuntime;
    }

    public bool HasConfirmedHit(CombatActionDefinition action)
    {
        if(action == null) return false;
        return chainRuntime.HasConfirmedHit(action);
    }
}
