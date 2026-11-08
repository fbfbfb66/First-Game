using UnityEngine;

[CreateAssetMenu(fileName = "Action_AI_", menuName = "Game/AI/Actions/Combat Action")]
public sealed class AICombatActionDefinition : CombatActionDefinition
{
    [SerializeField] private AICombatActionStage[] stages = System.Array.Empty<AICombatActionStage>();
    public override CombatActionStage[] CombatStages => stages;
}
