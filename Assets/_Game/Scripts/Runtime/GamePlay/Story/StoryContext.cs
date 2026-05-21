
using UnityEngine;

public readonly struct StoryContext 
{
    public StorySequenceRunner Runner {get;}
    public GameLayerStack LayerStack {get;}
    public GameFlagCenter FlagCenter {get;}
    public QuestManager QuestManager {get;}

    public GameObject Instigator {get;}

    public StoryContext(StorySequenceRunner runner,GameLayerStack layerStack,GameFlagCenter flagCenter,QuestManager questManager,GameObject instigator)
    {
        Runner = runner;
        LayerStack = layerStack;
        FlagCenter = flagCenter;
        QuestManager = questManager;
        Instigator = instigator;
    }
}
