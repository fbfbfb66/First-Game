
using UnityEngine;

public readonly struct StoryContext 
{
    public StorySequenceRunner Runner {get;}
    public GameLayerStack LayerStack {get;}
    public GameFlagCenter FlagCenter {get;}
    public QuestManager QuestManager {get;}
    public StorySceneBindings SceneBindings{get;}

    public GameObject Instigator {get;}

    public StoryContext(StorySequenceRunner runner,GameLayerStack layerStack,GameFlagCenter flagCenter,QuestManager questManager,StorySceneBindings sceneBindings,GameObject instigator)
    {
        Runner = runner;
        LayerStack = layerStack;
        FlagCenter = flagCenter;
        QuestManager = questManager;
        SceneBindings = sceneBindings;
        Instigator = instigator;
    }
}
