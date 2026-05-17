using UnityEngine;

public readonly struct GameFlagChangedEvent : IGameEvent
{
    public string FlagID{get;}
    public bool HadPreviousValue {get;}
    public bool PreviousValue {get;}
    public bool CurrentValue  {get;}
    public GameObject Sender {get;}
    public GameObject Instigator {get;}

    public GameFlagChangedEvent(string flagID,bool hadPreviousValue,bool previousValue,bool currentValue,GameObject sender,GameObject instigator)
    {
        FlagID = flagID;
        HadPreviousValue = hadPreviousValue;
        PreviousValue = previousValue;
        CurrentValue = currentValue;
        Sender = sender;
        Instigator = instigator;
    }
}
