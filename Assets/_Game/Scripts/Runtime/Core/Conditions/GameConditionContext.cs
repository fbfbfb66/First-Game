using UnityEngine;

public readonly struct GameConditionContext 
{
    public GameFlagCenter FlagCenter {get;}
    public GameObject Sender {get;}
    public GameObject Instigator {get;}

    public GameConditionContext(GameFlagCenter flagCenter,GameObject sender,GameObject instigator)
    {
        FlagCenter = flagCenter;
        Sender = sender;
        Instigator = instigator;
    }
}
