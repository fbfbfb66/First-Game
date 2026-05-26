using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WaitSecondsStoryStep",
    menuName = "Game/Story/Step/Wait Seconds")]
public class WaitSecondsStoryStepAction : StoryStepAction
{
    [SerializeField] private float duration = 1f;

    public override IEnumerator Execute(StoryContext context)
    {
        if(duration <= 0f)
        {
            yield break;
        }
        yield return new WaitForSeconds(duration);
    }
}
