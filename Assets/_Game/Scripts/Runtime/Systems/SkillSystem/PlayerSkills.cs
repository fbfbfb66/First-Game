using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerSkills
{
    private readonly HashSet<SkillData> unlockedSkills = new();
    public bool IsUnlocked(SkillData skill) => skill != null && unlockedSkills.Contains(skill);
    public bool TryUnlock(SkillData skill) => skill != null && unlockedSkills.Add(skill);
}
