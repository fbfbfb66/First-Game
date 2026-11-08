using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOptionSet_", menuName = "Game/AI/Utility/Option Set")]
public class AIUtilityOptionSetDefinition : ScriptableObject
{
    [SerializeField] private AIUtilityOptionDefinition[] options;

    public IReadOnlyList<AIUtilityOptionDefinition> Options => options;
}
