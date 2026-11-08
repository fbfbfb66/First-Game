using UnityEngine;

[CreateAssetMenu(fileName = "Action_", menuName = "Game/Actions/Combat/Action Definition")]
public sealed class PlayerCombatActionDefinition : CombatActionDefinition
{
    [SerializeField] private PlayerCombatActionStage[] stages = System.Array.Empty<PlayerCombatActionStage>();
    public override CombatActionStage[] CombatStages => stages;
}
