using System.Collections;
using UnityEngine;

public abstract class StoryStepBehaviour : MonoBehaviour
{
    public abstract IEnumerator Execute(StoryContext context);
}
