
using UnityEngine;

public readonly struct CombatActionContext
{
    public bool IsGrounded { get; }
    public bool CanEnterGrounded { get; }
    public Vector2 GroundProbePosition { get; }
    private readonly CombatActionChainRuntime chainRuntime;
    private readonly CombatActionUsageRuntime  usageRuntime;
    private readonly PlayerSkills skills;
    public CombatActionContext(
        bool isGrounded,
        bool canEnterGrounded,
        Vector2 groundProbePosition,
        CombatActionChainRuntime chainRuntime,
        CombatActionUsageRuntime usageRuntime,
        PlayerSkills skills = null)
    { 
        IsGrounded = isGrounded;
        CanEnterGrounded = canEnterGrounded;
        GroundProbePosition = groundProbePosition;
        this.chainRuntime = chainRuntime;
        this.usageRuntime = usageRuntime;
        this.skills = skills;
    }

    public bool IsSkillUnlocked(SkillData skill)
    {
        return skills != null && skill != null && skills.IsUnlocked(skill);
    }

    public bool HasActionBeenUsed(CombatActionDefinition action)
    {
        return usageRuntime.HasBeenUsed(action);
    }

    public bool HasConfirmedHit(CombatActionDefinition action)
    {
        if(action == null) return false;
        return chainRuntime.HasConfirmedHit(action);
    }
}
