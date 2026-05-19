using UnityEngine;

public class DebugQuestStateListener : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;

    private void Awake()
    {
        if (eventBus == null)
        {
            eventBus = FindAnyObjectByType<GameEventBus>();
        }
    }

    private void OnEnable()
    {
        if (eventBus == null)
        {
            return;
        }

        eventBus.Subscribe<QuestStateChangedEvent>(OnQuestStateChanged);
    }

    private void OnDisable()
    {
        if (eventBus == null)
        {
            return;
        }

        eventBus.Unsubscribe<QuestStateChangedEvent>(OnQuestStateChanged);
    }

    private void OnQuestStateChanged(QuestStateChangedEvent questEvent)
    {
        string questTitle = questEvent.QuestData != null
            ? questEvent.QuestData.Title
            : questEvent.QuestID;

        Debug.Log(
            $"Quest state changed. " +
            $"Quest: {questTitle}, " +
            $"Previous: {questEvent.PreviousState}, " +
            $"Current: {questEvent.CurrentState}"
        );
    }
}