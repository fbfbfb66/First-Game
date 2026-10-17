using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    private readonly HashSet<SkillData> unlockedSkills = new();
    [SerializeField] private SkillData debugSkill;

    public bool IsUnlocked(SkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Skill is null");
            return false;
        }
        return unlockedSkills.Contains(skill);
    }

    public bool TryUnlock(SkillData skill)
    {
        throw new System.NotImplementedException();
    }

    [ContextMenu("Debug/Unlock Skill")]
    public void DebugUnlockSkill()
    {
        unlockedSkills.Add(debugSkill);
        Debug.Log($"{debugSkill.DisplayName}  has been unlocked");
    }
}
