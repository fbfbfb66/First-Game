using UnityEngine;

[CreateAssetMenu(fileName = "Action_", menuName = "Game/Actions/Movement Action")]
public sealed class MotionActionDefinition : ActionDefinition
{
    [SerializeField] private bool needCommand = true;
    [SerializeField] private ActionStage stage = new();

    public override ActionStage Stage => stage;
    public bool NeedCommand => needCommand;
}
