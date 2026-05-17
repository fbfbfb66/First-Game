using UnityEngine;
[CreateAssetMenu(
    fileName = "DebugLogDialogueEvent",
    menuName = "Game/Dialogue/Event/Debug Log")]
public class DebugLogDialogueEvent : DialogueEventAction
{
    [SerializeField] private string Message;
    public override void Execute(DialogueContext context)
    {
        Debug.Log($"Debug Log Dialogue Event: {Message}");
    }

}
