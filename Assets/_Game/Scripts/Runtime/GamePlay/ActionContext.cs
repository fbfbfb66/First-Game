using UnityEngine;

public readonly struct ActionContext
{
    public bool IsGrounded { get; }
    public bool CanEnterGrounded { get; }
    public Vector2 GroundProbePosition { get; }
    public bool CanDash { get; }
    public bool CanDoubleJump { get; }
    private readonly PlayerSkills skills;

    public ActionContext(bool isGrounded, bool canEnterGrounded, Vector2 groundProbePosition, bool canDash = false,
        bool canDoubleJump = false, PlayerSkills skills = null)
    {
        IsGrounded = isGrounded;
        CanEnterGrounded = canEnterGrounded;
        GroundProbePosition = groundProbePosition;
        CanDash = canDash;
        CanDoubleJump = canDoubleJump;
        this.skills = skills;
    }

    public bool IsSkillUnlocked(SkillData skill)
    {
        return skills != null && skill != null && skills.IsUnlocked(skill);
    }
}
