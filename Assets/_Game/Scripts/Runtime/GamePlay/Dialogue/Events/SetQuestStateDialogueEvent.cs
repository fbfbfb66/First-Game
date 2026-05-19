using UnityEngine;
[CreateAssetMenu(
    fileName = "SetQuestStateDialogueEvent",
    menuName = "Game/Dialogue/Event/Set Quest State")]
public class SetQuestStateDialogueEvent : DialogueEventAction
{
    [SerializeField] private QuestData questData;
    [SerializeField] private QuestState targetState = QuestState.InProgress;
    public override void Execute(DialogueContext context)
    {
        if (context.QuestManager == null)
        {
            Debug.LogWarning("Cannot set quest state. QuestManager is missing.");
            return;
        }

        if (questData == null)
        {
            Debug.LogWarning("Cannot set quest state. QuestData is null.");
            return;
        }

        context.QuestManager.SetQuestState(
            questData,
            targetState,
            context.Sender,
            context.Instigator
        );
    }
}
