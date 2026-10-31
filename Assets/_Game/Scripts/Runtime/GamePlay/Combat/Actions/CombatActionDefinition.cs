using UnityEngine;
[CreateAssetMenu(fileName ="Action_",menuName ="Game/Actions/Combat/Action Definition")]
public sealed class CombatActionDefinition : ActionDefinition
{
    [SerializeField] private CombatActionStage[] stages = System.Array.Empty<CombatActionStage>();
    [SerializeField] private CombatActionTransition[] transitions;


    public override ActionStage[] Stages => stages;
    public CombatActionStage[] CombatStages => stages;
    public CombatActionTransition[] Transitions => transitions;


}
