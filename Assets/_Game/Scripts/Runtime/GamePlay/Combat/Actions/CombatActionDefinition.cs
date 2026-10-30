using UnityEngine;
[CreateAssetMenu(fileName ="Action_",menuName ="Game/Actions/Combat/Action Definition")]
public sealed class CombatActionDefinition : ActionDefinition
{
    [SerializeField] private CombatActionStage stage;
    [SerializeField] private CombatActionTransition[] transitions;


    public override ActionStage Stage => stage;
    public CombatActionStage CombatStage => stage;
    public CombatActionTransition[] Transitions => transitions;


}
