using UnityEngine;
[CreateAssetMenu(fileName ="Action_",menuName ="Game/Combat/Action Definition")]
public sealed class CombatActionDefinition : ScriptableObject
{
    [SerializeField] private CombatActionCommandKey command;
    [SerializeField] private CombatActionStage stage;

    [SerializeField] private CombatActionTransition transition;


    public CombatActionCommandKey Command => command;
    public CombatActionStage Stage => stage;
    public CombatActionTransition Transition => transition;
}
