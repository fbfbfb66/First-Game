using UnityEngine;

public readonly struct GameSignalEvent : IGameEvent
{
    public string SignalID {get;}
    public GameObject Sender {get;}
    public GameObject Instigator {get;}
    public GameSignalEvent(string signalID, GameObject sender, GameObject instigator)
    {
        SignalID = signalID;
        Sender = sender;
        Instigator = instigator;
    }
}
