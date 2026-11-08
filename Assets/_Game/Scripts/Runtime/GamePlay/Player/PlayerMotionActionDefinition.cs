using UnityEngine;

[CreateAssetMenu(fileName = "Action_", menuName = "Game/Actions/Movement Action")]
public sealed class PlayerMotionActionDefinition : MotionActionDefinition
{
    [SerializeField] private bool needCommand = true;
    [SerializeField] private PlayerActionStage[] stages = System.Array.Empty<PlayerActionStage>();
    public override ActionStage[] Stages => stages;
    public override bool NeedCommand => needCommand;
}
