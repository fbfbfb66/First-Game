using System.Collections;
using UnityEngine;

public abstract class StoryStepAction : ScriptableObject
{
    public abstract IEnumerator Execute(StoryContext context);
}
