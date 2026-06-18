using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "PopGameLayerStoryStep",
    menuName = "Game/Story/Step/Pop Game Layer")]
public class PopGameLayerStoryStepAction : StoryStepAction
{
    [SerializeField] private GameLayerType layerType = GameLayerType.Cutscene;
    public override IEnumerator Execute(StoryContext context)
    {
        if(context.LayerStack == null)
        {
            Debug.LogWarning("LayerStack is null in context");
            yield break;
        }
        context.LayerStack.PopLayer(layerType);
        yield break;
    }
}
