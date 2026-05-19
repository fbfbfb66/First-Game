using UnityEngine;
[CreateAssetMenu(
    fileName = "QuestData",
    menuName = "Game/Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [SerializeField] private string questID;
    [SerializeField] private string title;

    [TextArea(2,5)]
    [SerializeField] private string descrition;
    [SerializeField] private QuestState defaultState = QuestState.NotStarted;

    public string QuestID => questID;
    public string Title => title;
    public string Description => descrition;
    public QuestState DefaultState => defaultState;
}
