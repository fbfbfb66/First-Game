using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "PushGameLayerStoryStep",
    menuName = "Game/Story/Step/Push Game Layer")]
public class PushGameLayerStoryStepAction : StoryStepAction
{
    [SerializeField] private GameLayerType layerType = GameLayerType.Cutscene;
    public override IEnumerator Execute(StoryContext context)
    {
        if(context.LayerStack == null)
        {
            Debug.LogWarning("LayerStack is null in context");
            yield break;
        }
        context.LayerStack.PushLayer(layerType);
        yield break;
    }
}
