using UnityEngine;

[CreateAssetMenu(fileName = "Action_AI_", menuName = "Game/AI/Actions/Motion Action")]
public sealed class AIMotionActionDefinition : MotionActionDefinition
{
    [SerializeField] private AIActionStage[] stages = System.Array.Empty<AIActionStage>();
    public override ActionStage[] Stages => stages;
    public override bool NeedCommand => false;
}
