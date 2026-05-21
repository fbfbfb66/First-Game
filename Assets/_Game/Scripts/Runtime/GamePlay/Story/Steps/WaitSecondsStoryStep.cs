using System.Collections;
using UnityEngine;

public class WaitSecondsStoryStep : StoryStepBehaviour
{
    [SerializeField] private float duration = 1f;

    public override IEnumerator Execute(StoryContext context)
    {
        if(duration <= 0)
        {
            yield break;
        }
        yield return new WaitForSeconds(duration);
    }
}
