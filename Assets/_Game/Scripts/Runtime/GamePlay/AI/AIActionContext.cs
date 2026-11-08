/// <summary>
/// Facts supplied when AI executes an external action. A default context has no damage event.
/// </summary>
public readonly struct AIActionContext
{
    public DamageContext? DamageContext { get; }

    public AIActionContext(DamageContext damageContext)
    {
        DamageContext = damageContext;
    }
}
