using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "HideStoryTextStep",
    menuName = "Game/Story/Step/Hide Story Text")]
public class HideStoryTextStepAction : StoryStepAction
{
    [SerializeField] private StoryBindingKey textViewKey;
    public override IEnumerator Execute(StoryContext context)
    {
        if (context.SceneBindings == null)
        {
            Debug.LogWarning("Cannot set player move mode. StorySceneBindings is missing.");
            yield break;
        }
        StoryTextView textView = context.SceneBindings.GetComponent<StoryTextView>(textViewKey);

        if(textView == null) yield break;
        textView.Hide();
        yield break;
    }
}
