using UnityEngine;
[CreateAssetMenu(
    fileName = "SetFlagDialogueEvent",
    menuName = "Game/Dialogue/Event/Set Bool Flag")]
public class SetFlagDialogueEvent : DialogueEventAction
{
    [SerializeField] private GameFlagData flagData;
    [SerializeField] private bool value;
    public override void Execute(DialogueContext context)
    {
        if(context.FlagCenter == null)
        {
            Debug.LogWarning("Missing FlagCenter.");
            return;
        }
        context.FlagCenter.SetBool(flagData.FlagID,value,context.Sender,context.Instigator);
    }
}
