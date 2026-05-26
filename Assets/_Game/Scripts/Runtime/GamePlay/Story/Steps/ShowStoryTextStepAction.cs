using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "ShowStoryTextStep",
    menuName = "Game/Story/Step/Show Story Text")]
public class ShowStoryTextStepAction : StoryStepAction
{
    [SerializeField] private StoryBindingKey textViewKey;
    [SerializeField] private string content;

    public override IEnumerator Execute(StoryContext context)
    {
        if (context.SceneBindings == null)
        {
            Debug.LogWarning("Cannot set player move mode. StorySceneBindings is missing.");
            yield break;
        }
        StoryTextView textView = context.SceneBindings.GetComponent<StoryTextView>(textViewKey);

        if(textView == null) yield break;
        textView.ShowText(content);
        yield break;
    }
}
