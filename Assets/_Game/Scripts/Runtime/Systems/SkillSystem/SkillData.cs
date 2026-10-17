using UnityEngine;
[CreateAssetMenu(fileName = "Skill_",
    menuName = "Game/Skills/Skill")]
public sealed class SkillData : ScriptableObject
{
    [SerializeField] private string displayName;
    
    public string DisplayName => displayName;
}
