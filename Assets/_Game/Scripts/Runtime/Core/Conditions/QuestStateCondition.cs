using UnityEngine;
[CreateAssetMenu(
    fileName = "QuestStateCondition",
    menuName = "Game/Condition/Quest State")]
public class QuestStateCondition : GameCondition
{
    [SerializeField] private QuestData questData;
    [SerializeField] private QuestState expectedState = QuestState.NotStarted;
    public override bool IsMet(GameConditionContext context)
    {
        if(questData == null)
        {
            Debug.LogWarning("questData is Null.");
            return false;
        }
        if(context.QuestManager == null)
        {
            Debug.LogWarning("Missing questManager.");
            return false;
        }
        if (!context.QuestManager.HasQuest(questData))
        {
            Debug.LogWarning($"Cannot check quest state condition. Quest is not registered. Quest: {questData.name}");
            return false;
        }
        return expectedState == context.QuestManager.GetQuestState(questData);
    }
}
