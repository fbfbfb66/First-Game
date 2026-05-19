
using UnityEngine;

public readonly struct QuestStateChangedEvent : IGameEvent
{
    public QuestData QuestData {get;}
    public string QuestID {get;}

    public QuestState PreviousState {get;}
    public QuestState CurrentState {get;}

    public GameObject Sender {get;}
    public GameObject Instigator {get;}

    public QuestStateChangedEvent(QuestData questData,string questID,QuestState previousState,QuestState currentState,GameObject sender,GameObject insigator)
    {
        QuestData = questData;
        QuestID = questID;
        PreviousState = previousState;
        CurrentState = currentState;
        Sender = sender;
        Instigator = insigator;
    }
}
