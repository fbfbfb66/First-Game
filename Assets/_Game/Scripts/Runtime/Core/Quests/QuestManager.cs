using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestDatabase questDatabase;
    [SerializeField] private GameEventBus eventBus;

    private readonly Dictionary<string,QuestData> questDataByID = new();
    private readonly Dictionary<string,QuestState> questStates = new();

    private void Awake()
    {
        if(eventBus == null)
            eventBus = FindAnyObjectByType<GameEventBus>();
        
        InitializeQuests();
    }

    public bool HasQuest(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID))
        {
            return false;
        }
        return questStates.ContainsKey(questID);
    }

    public bool HasQuest(QuestData questData)
    {
        if(questData == null)
        {
            return false;
        }
        return HasQuest(questData.QuestID);
    }

    public QuestState GetQuestState(QuestData questData)
    {
        if(questData == null)
        {
            Debug.LogWarning("Cannot get quest state. QuestData is null.");
            return QuestState.NotStarted;
        }
        return GetQuestState(questData.QuestID);
    }

    public QuestState GetQuestState(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID))
        {
            Debug.LogWarning("Cannot get quest state. QuestId is empty.");
            return QuestState.NotStarted;
        }

        if(questStates.TryGetValue(questID,out var state))
        {
            return state;
        }
        Debug.LogWarning($"Quest state not found. QuestId: {questID}");
        return QuestState.NotStarted;
    }
    public void SetQuestState(QuestData questData,QuestState newState,GameObject sender = null,GameObject instigator = null)
    {
        if(questData == null)
        {
            Debug.LogWarning("Cannot set quest state. QuestData is null.");
            return;
        }

        string questID = questData.QuestID;

        if (string.IsNullOrWhiteSpace(questID))
        {
            Debug.LogWarning($"Cannot set quest state. QuestId is empty. Quest asset: {questData.name}");
            return;
        }
        if(!questStates.TryGetValue(questID,out var previousState))
        {
            Debug.LogWarning($"Cannot set quest state. Quest is not registered in QuestManager. QuestId: {questID}");
            return;
        }
        if(previousState == newState) return;
        questStates[questID] = newState;

        if(eventBus != null)
        {
            eventBus.Publish(new QuestStateChangedEvent(questData,questID,previousState,newState,sender,instigator));
        }
    }

    private void InitializeQuests()
    {
        questDataByID.Clear();
        questStates.Clear();
         if (questDatabase == null)
        {
            Debug.LogWarning("QuestManager has no QuestDatabase. It will start with an empty quest table.");
            return;
        }

        IReadOnlyList<QuestData> quests = questDatabase.QuestDatas;

        for (int i = 0; i < quests.Count; i++)
        {
            QuestData questData = quests[i];

            if (questData == null)
            {
                Debug.LogWarning($"QuestDatabase has a null QuestData at index {i}.");
                continue;
            }

            string questID = questData.QuestID;

            if (string.IsNullOrWhiteSpace(questID))
            {
                Debug.LogWarning($"QuestData has an empty QuestId. Asset: {questData.name}");
                continue;
            }

            if (questDataByID.ContainsKey(questID))
            {
                Debug.LogWarning($"Duplicate QuestId found in QuestDatabase. QuestId: {questID}");
                continue;
            }

            questDataByID.Add(questID, questData);
            questStates.Add(questID, questData.DefaultState);
        }
    }
}
