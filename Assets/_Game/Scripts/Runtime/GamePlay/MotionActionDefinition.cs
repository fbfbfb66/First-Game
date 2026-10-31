using UnityEngine;

[CreateAssetMenu(fileName = "Action_", menuName = "Game/Actions/Movement Action")]
public sealed class MotionActionDefinition : ActionDefinition
{
    [SerializeField] private bool needCommand = true;
    [SerializeField] private ActionStage[] stages = System.Array.Empty<ActionStage>();

    public override ActionStage[] Stages => stages;
    public bool NeedCommand => needCommand;
}
