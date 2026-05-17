using UnityEngine;
[CreateAssetMenu(
    fileName = "SetFlagDialogueEvent",
    menuName = "Game/Dialogue/Event/Set Bool Flag")]
public class SetFlagDialogueEvent : DialogueEventAction
{
    [SerializeField] private string flagID;
    [SerializeField] private bool value;
    public override void Execute(DialogueContext context)
    {
        if(context.FlagCenter == null)
        {
            Debug.LogWarning("Missing FlagCenter.");
            return;
        }
        context.FlagCenter.SetBool(flagID,value,context.Sender,context.Instigator);
    }
}
