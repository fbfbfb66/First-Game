using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "HideStoryTextStep",
    menuName = "Game/Story/Step/Debug Story Text")]
public class DebugLogStoryStep : StoryStepAction
{
    [SerializeField] private string message;
    public override IEnumerator Execute(StoryContext context)
    {
        Debug.Log($"Story Step {message}");
        yield break;
    }
}
