using System.Collections;
using UnityEngine;

public class DebugLogStoryStep : StoryStepBehaviour
{
    [SerializeField] private string message;
    public override IEnumerator Execute(StoryContext context)
    {
        Debug.Log($"Story Step {message}");
        yield break;
    }
}
