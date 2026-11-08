using UnityEngine;

public abstract class CombatActionDefinition : ActionDefinition
{
    [SerializeField] private CombatActionTransition[] transitions;
    [SerializeField, Min(0f)] private float cooldownDuration;
    public abstract CombatActionStage[] CombatStages { get; }
    public override ActionStage[] Stages => CombatStages;
    public CombatActionTransition[] Transitions => transitions;
    public float CooldownDuration => cooldownDuration;
}
