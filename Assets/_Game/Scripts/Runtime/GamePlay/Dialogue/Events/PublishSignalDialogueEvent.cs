using UnityEngine;
[CreateAssetMenu(
    fileName = "PublishSignalDialogueEvent",
    menuName = "Game/Dialogue/Event/Publish Signal")]
public class PublishSignalDialogueEvent : DialogueEventAction
{
    [SerializeField] private string SignalID;
    public override void Execute(DialogueContext context)
    {
        if(context.EventBus == null)
        {
            Debug.LogWarning("PublishSignalDialogueEvent: EventBus is null. Cannot publish signal.");
            return;
        }
        context.EventBus.Publish(new GameSignalEvent(SignalID, context.Sender, context.Instigator));
    }

}
